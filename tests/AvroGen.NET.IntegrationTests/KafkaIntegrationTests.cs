using Avro;
using Avro.Specific;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.IO;
using System.Net;
using Confluent.Kafka.Admin;
using Xunit;
using Schema = Avro.Schema;

namespace AvroGen.NET.IntegrationTests;

/// <summary>
/// Кастомный сериализатор для Avro объектов.
/// Нам нужен отдельный сериализатор, потому что стандартный AvroSerializer не поддерживает
/// работу с динамически созданными типами. Кроме того, нам нужно правильно форматировать
/// данные в формате Confluent (магический байт + ID схемы + данные).
/// </summary>
public class DynamicAvroSerializer : ISerializer<object>
{
    private readonly ISchemaRegistryClient _schemaRegistry;
    private readonly string _topic;
    private readonly string _schemaStr;

    public DynamicAvroSerializer(ISchemaRegistryClient schemaRegistry, string topic, string schemaStr)
    {
        _schemaRegistry = schemaRegistry;
        _topic = topic;
        _schemaStr = schemaStr;
    }

    public byte[] Serialize(object data, SerializationContext context)
    {
        if (data is ISpecificRecord record)
        {
            using var stream = new MemoryStream();
            var writer = new Avro.IO.BinaryEncoder(stream);
            var schema = Schema.Parse(_schemaStr);
            var datumWriter = new SpecificWriter<ISpecificRecord>(schema);
            
            // Регистрируем схему в Schema Registry, чтобы получить её ID.
            // Это нужно для того, чтобы потребитель мог найти схему по ID и правильно десериализовать данные.
            var subject = $"{_topic}-value";
            var id = _schemaRegistry.RegisterSchemaAsync(subject, _schemaStr).Result;
            
            // Записываем данные в формате Confluent:
            // Байт 0: Магический байт (всегда 0)
            // Байты 1-4: ID схемы (в формате big-endian)
            // Остальные байты: сериализованные данные
            stream.WriteByte(0);
            var idBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(id));
            stream.Write(idBytes, 0, idBytes.Length);
            
            datumWriter.Write(record, writer);
            return stream.ToArray();
        }
        
        throw new ArgumentException("Data must be ISpecificRecord");
    }
}

/// <summary>
/// Кастомный десериализатор для Avro объектов.
/// Нужен для корректной десериализации данных в динамически созданный тип.
/// Стандартный десериализатор не может работать с динамическими типами,
/// поэтому мы реализуем свой, который знает как создать объект нужного типа.
/// </summary>
public class DynamicAvroDeserializer : IDeserializer<object>
{
    private readonly ISchemaRegistryClient _schemaRegistry;
    private readonly Type _recordType;

    public DynamicAvroDeserializer(ISchemaRegistryClient schemaRegistry, Type recordType)
    {
        _schemaRegistry = schemaRegistry;
        _recordType = recordType;
    }

    public object Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull) return null;

        using var stream = new MemoryStream(data.ToArray());
        
        // Читаем заголовок в формате Confluent:
        // - Магический байт (пропускаем)
        // - ID схемы (4 байта в формате big-endian)
        var magicByte = stream.ReadByte();
        var idBytes = new byte[4];
        stream.Read(idBytes, 0, 4);
        var id = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(idBytes, 0));
        
        // Получаем схему из реестра по её ID
        var schema = Schema.Parse(_schemaRegistry.GetSchemaAsync(id).Result);
        
        var reader = new Avro.IO.BinaryDecoder(stream);
        var datumReader = new SpecificReader<ISpecificRecord>(schema, schema);
        
        // Создаем экземпляр динамического типа и десериализуем в него данные
        var result = Activator.CreateInstance(_recordType);
        datumReader.Read((ISpecificRecord)result, reader);
        
        return result;
    }
}

/// <summary>
/// Интеграционный тест для проверки работы с Kafka и Avro.
/// Проверяет полный цикл:
/// 1. Создание схемы и её регистрация в Schema Registry
/// 2. Генерация .NET класса из Avro схемы
/// 3. Компиляция сгенерированного кода "на лету"
/// 4. Отправка и получение сообщений через Kafka с использованием Avro сериализации
/// </summary>
public class KafkaIntegrationTests : IAsyncLifetime
{
    // Конфигурация для подключения к локальным сервисам
    private const string SchemaRegistryUrl = "http://localhost:8081";
    private const string BootstrapServers = "localhost:9092";
    private const string TopicName = "test-topic";

    // Клиенты для работы с различными сервисами
    private ISchemaRegistryClient? _schemaRegistry;
    private IProducer<string, object>? _producer;
    private IConsumer<string, object>? _consumer;
    
    // Инструменты для работы с Avro
    private readonly AvroCodeGenerator _generator;
    private Assembly? _generatedAssembly;
    private Type? _testRecordType;
    private string? _schemaStr;

    public KafkaIntegrationTests()
    {
        _generator = new AvroCodeGenerator();
    }

    public async Task InitializeAsync()
    {
        // Инициализируем Schema Registry клиент
        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            Url = SchemaRegistryUrl
        };

        _schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

        // Создаем топик в Kafka, если он еще не существует.
        // Это важно сделать до начала работы с топиком, иначе получим ошибку.
        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = BootstrapServers
        };

        using (var adminClient = new AdminClientBuilder(adminConfig).Build())
        {
            try
            {
                await adminClient.CreateTopicsAsync(new TopicSpecification[]
                {
                    new TopicSpecification
                    {
                        Name = TopicName,
                        ReplicationFactor = 1, // Для локальной разработки достаточно одной реплики
                        NumPartitions = 1      // И одной партиции
                    }
                }, new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(30) });
            }
            catch (CreateTopicsException e) when (e.Results.Select(r => r.Error.Code).All(code => code == ErrorCode.TopicAlreadyExists))
            {
                // Игнорируем ошибку, если топик уже существует
            }
        }

        // Определяем Avro схему для тестового сообщения
        _schemaStr = @"{
            ""type"": ""record"",
            ""name"": ""TestRecord"",
            ""namespace"": ""AvroGen.NET.IntegrationTests"",
            ""fields"": [
                { ""name"": ""Name"", ""type"": ""string"" },
                { ""name"": ""Age"", ""type"": ""int"" }
            ]
        }";

        var avroSchema = Schema.Parse(_schemaStr);
        var subject = $"{TopicName}-value";
        await _schemaRegistry.RegisterSchemaAsync(subject, _schemaStr);

        // Генерируем C# код из Avro схемы
        var generatedCode = _generator.GenerateCode(avroSchema);

        // Компилируем сгенерированный код с помощью Roslyn.
        // Это позволяет нам создавать типы "на лету" без необходимости
        // предварительной компиляции и деплоя сборок.
        var syntaxTrees = generatedCode.Values.Select(code => 
            CSharpSyntaxTree.ParseText(code)).ToArray();

        // Добавляем все необходимые ссылки на сборки
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ISpecificRecord).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Schema).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
            MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
            MetadataReference.CreateFromFile(typeof(AvroRuntimeException).Assembly.Location)
        };

        // Добавляем все сборки из текущего домена приложения
        // Это нужно, потому что сгенерированный код может зависеть
        // от разных системных типов
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }
            catch
            {
                // Пропускаем сборки, которые не можем загрузить
            }
        }

        // Компилируем код в память
        var compilation = CSharpCompilation.Create(
            "DynamicAssembly",
            syntaxTrees,
            references.Distinct(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var failures = result.Diagnostics.Where(diagnostic => 
                diagnostic.IsWarningAsError || 
                diagnostic.Severity == DiagnosticSeverity.Error);

            var errors = string.Join(Environment.NewLine, failures);
            throw new Exception($"Compilation failed: {errors}");
        }

        // Загружаем скомпилированную сборку и получаем из неё наш тип
        ms.Seek(0, SeekOrigin.Begin);
        _generatedAssembly = Assembly.Load(ms.ToArray());
        _testRecordType = _generatedAssembly.GetType("AvroGen.NET.IntegrationTests.TestRecord");

        if (_testRecordType == null)
        {
            throw new Exception("Failed to load TestRecord type from generated assembly");
        }

        // Настраиваем producer для отправки сообщений
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = BootstrapServers
        };

        // Используем наш кастомный сериализатор для корректной работы с Avro
        var producerBuilder = new ProducerBuilder<string, object>(producerConfig)
            .SetValueSerializer(new DynamicAvroSerializer(_schemaRegistry, TopicName, _schemaStr));

        _producer = producerBuilder.Build();

        // Настраиваем consumer для чтения сообщений
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest // Читаем с самого начала топика
        };

        // Используем наш кастомный десериализатор
        var consumerBuilder = new ConsumerBuilder<string, object>(consumerConfig)
            .SetValueDeserializer(new DynamicAvroDeserializer(_schemaRegistry, _testRecordType));

        _consumer = consumerBuilder.Build();
        _consumer?.Subscribe(TopicName);
    }

    public Task DisposeAsync()
    {
        // Освобождаем ресурсы
        if (_producer != null)
        {
            _producer.Dispose();
        }

        if (_consumer != null)
        {
            _consumer.Dispose();
        }

        if (_schemaRegistry != null)
        {
            _schemaRegistry.Dispose();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Тест проверяет полный цикл работы с Kafka и Avro:
    /// 1. Создание объекта динамического типа
    /// 2. Сериализация и отправка в Kafka
    /// 3. Получение из Kafka и десериализация
    /// 4. Проверка, что данные совпадают
    /// </summary>
    [Fact]
    public async Task ProduceAndConsume_WithAvroSerialization_WorksCorrectly()
    {
        // Arrange: создаем и заполняем тестовый объект
        var testRecord = Activator.CreateInstance(_testRecordType!) as ISpecificRecord;
        Assert.NotNull(testRecord);

        testRecord!.Put(0, "Test Name");  // Name
        testRecord.Put(1, 25);           // Age

        // Act: отправляем сообщение в Kafka
        await _producer!.ProduceAsync(TopicName, new Message<string, object>
        {
            Key = "test-key",
            Value = testRecord
        });

        // Act: читаем сообщение из Kafka
        var consumeResult = _consumer!.Consume(TimeSpan.FromSeconds(10));

        // Assert: проверяем, что данные совпадают
        Assert.NotNull(consumeResult);
        Assert.Equal("test-key", consumeResult.Message.Key);
        
        var consumedRecord = consumeResult.Message.Value as ISpecificRecord;
        Assert.NotNull(consumedRecord);
        Assert.Equal("Test Name", consumedRecord.Get(0));
        Assert.Equal(25, consumedRecord.Get(1));
    }
}
