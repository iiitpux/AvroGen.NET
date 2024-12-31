using Avro;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace AvroGen.NET;

/// <summary>
/// Генератор кода для преобразования Avro схем в C# классы.
/// </summary>
public class AvroCodeGenerator
{
    /// <summary>
    /// Генерирует C# код из Avro схемы.
    /// </summary>
    /// <param name="schema">Avro схема</param>
    /// <returns>Сгенерированный C# код</returns>
    public string GenerateCode(Schema schema)
    {
        if (schema == null)
        {
            throw new ArgumentNullException(nameof(schema));
        }

        if (!(schema is RecordSchema))
        {
            throw new ArgumentException("Schema must be a record schema", nameof(schema));
        }

        var codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
        var codeUnit = new CodeCompileUnit();

        // Добавляем импорты
        var codeNamespace = new CodeNamespace();
        codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
        codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
        codeNamespace.Imports.Add(new CodeNamespaceImport("Avro"));
        codeUnit.Namespaces.Add(codeNamespace);

        // Создаем основное пространство имен
        var mainNamespace = new CodeNamespace(GetNamespace(schema));
        codeUnit.Namespaces.Add(mainNamespace);

        // Создаем классы для всех типов записей в схеме
        GenerateRecordClasses(schema, mainNamespace);

        // Генерируем код
        var options = new CodeGeneratorOptions
        {
            BracingStyle = "C",
            IndentString = "    "
        };

        using var writer = new StringWriter();
        codeProvider.GenerateCodeFromCompileUnit(codeUnit, writer, options);
        return writer.ToString();
    }

    /// <summary>
    /// Получает имя класса из схемы.
    /// </summary>
    /// <param name="schema">Avro схема</param>
    /// <returns>Имя класса</returns>
    private string GetClassName(Schema schema)
    {
        if (schema is RecordSchema recordSchema)
        {
            var fullName = recordSchema.Name;
            var lastDot = fullName.LastIndexOf('.');
            if (lastDot > 0)
            {
                return fullName.Substring(lastDot + 1);
            }
            return fullName;
        }
        return schema.Name ?? "GeneratedClass";
    }

    private string GetNamespace(Schema schema)
    {
        if (schema is RecordSchema recordSchema)
        {
            var fullName = recordSchema.Name;
            var lastDot = fullName.LastIndexOf('.');
            if (lastDot > 0)
            {
                return fullName.Substring(0, lastDot);
            }
            return recordSchema.Namespace ?? "Generated";
        }
        return "Generated";
    }

    private void GenerateRecordClasses(Schema schema, CodeNamespace codeNamespace)
    {
        if (schema is RecordSchema recordSchema)
        {
            var codeClass = new CodeTypeDeclaration(GetClassName(schema))
            {
                IsClass = true,
                TypeAttributes = System.Reflection.TypeAttributes.Public
            };

            // Добавляем поля
            foreach (var field in recordSchema.Fields)
            {
                var codeField = new CodeMemberField
                {
                    Name = field.Name,
                    Type = GetCodeTypeReference(field.Schema),
                    Attributes = MemberAttributes.Public
                };
                codeClass.Members.Add(codeField);
            }

            // Добавляем методы сериализации
            AddSerializationMethods(codeClass);

            codeNamespace.Types.Add(codeClass);

            // Рекурсивно обрабатываем вложенные типы
            foreach (var field in recordSchema.Fields)
            {
                if (field.Schema is ArraySchema arraySchema && arraySchema.ItemSchema is RecordSchema)
                {
                    GenerateRecordClasses(arraySchema.ItemSchema, codeNamespace);
                }
                else if (field.Schema is RecordSchema nestedRecordSchema)
                {
                    GenerateRecordClasses(field.Schema, codeNamespace);
                }
            }
        }
    }

    private void AddSerializationMethods(CodeTypeDeclaration codeClass)
    {
        // Write метод
        var writeMethod = new CodeMemberMethod
        {
            Name = "Write",
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
            ReturnType = new CodeTypeReference(typeof(void))
        };
        writeMethod.Parameters.Add(new CodeParameterDeclarationExpression("Encoder", "encoder"));
        codeClass.Members.Add(writeMethod);

        // Read метод
        var readMethod = new CodeMemberMethod
        {
            Name = "Read",
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
            ReturnType = new CodeTypeReference(typeof(void))
        };
        readMethod.Parameters.Add(new CodeParameterDeclarationExpression("Decoder", "decoder"));
        codeClass.Members.Add(readMethod);
    }

    private CodeTypeReference GetCodeTypeReference(Schema schema)
    {
        return schema.Tag switch
        {
            Schema.Type.String => new CodeTypeReference(typeof(string)),
            Schema.Type.Int => new CodeTypeReference(typeof(int)),
            Schema.Type.Long => new CodeTypeReference(typeof(long)),
            Schema.Type.Float => new CodeTypeReference(typeof(float)),
            Schema.Type.Double => new CodeTypeReference(typeof(double)),
            Schema.Type.Boolean => new CodeTypeReference(typeof(bool)),
            Schema.Type.Array => new CodeTypeReference(
                typeof(System.Collections.Generic.List<>).FullName,
                new[] { GetCodeTypeReference(((ArraySchema)schema).ItemSchema) }
            ),
            Schema.Type.Record => new CodeTypeReference(GetClassName(schema)),
            Schema.Type.Union => GetCodeTypeReferenceForUnion(schema),
            _ => throw new NotSupportedException($"Schema type {schema.Tag} is not supported")
        };
    }

    private CodeTypeReference GetCodeTypeReferenceForUnion(Schema schema)
    {
        var unionSchema = (UnionSchema)schema;
        if (unionSchema.Count == 2 && unionSchema[0].Tag == Schema.Type.Null)
        {
            var valueType = GetCodeTypeReference(unionSchema[1]);
            return new CodeTypeReference(typeof(Nullable<>).Name, new[] { valueType });
        }
        throw new NotSupportedException("Only nullable unions are supported");
    }
}
