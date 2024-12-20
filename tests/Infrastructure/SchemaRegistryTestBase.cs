using Confluent.SchemaRegistry;
using Xunit;

namespace AvroGen.NET.Test.Infrastructure;

public abstract class SchemaRegistryTestBase : IAsyncLifetime
{
    protected readonly string SchemaRegistryUrl = "http://localhost:8081";
    protected ISchemaRegistryClient SchemaRegistry;
    
    protected SchemaRegistryTestBase()
    {
        var config = new SchemaRegistryConfig { Url = SchemaRegistryUrl };
        SchemaRegistry = new CachedSchemaRegistryClient(config);
    }

    public virtual Task InitializeAsync()
    {
        // Wait for Schema Registry to be ready
        return Task.Delay(2000);
    }

    public virtual Task DisposeAsync()
    {
        SchemaRegistry?.Dispose();
        return Task.CompletedTask;
    }
}
