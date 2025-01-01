using Avro;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using Avro.Specific;
using Microsoft.CSharp;
using System.Runtime.InteropServices;
using Avro.Util;

namespace AvroGen.NET
{
    /// <summary>
    /// Code generator for converting Avro schemas to C# classes.
    /// </summary>
    public class AvroCodeGenerator
    {
        private readonly Dictionary<string, CodeCompileUnit> _compileUnits = new();
        private readonly HashSet<string> _processedTypes = new();

        /// <summary>
        /// Generates C# code from an Avro schema.
        /// </summary>
        /// <param name="schema">Avro schema</param>
        /// <param name="namespace">Namespace for generated classes</param>
        /// <returns>Dictionary where key is filename and value is generated code</returns>
        public Dictionary<string, string> GenerateCode(Schema schema, string? @namespace = null)
        {
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            if (!(schema is RecordSchema))
                throw new ArgumentException("Schema must be of type record", nameof(schema));

            _compileUnits.Clear();
            _processedTypes.Clear();

            // Process schema
            ProcessSchema(schema, @namespace ?? GetNamespace(schema));

            var result = new Dictionary<string, string>();
            var provider = new CSharpCodeProvider();
            var options = new CodeGeneratorOptions
            {
                BracingStyle = "C",
                IndentString = "    "
            };

            foreach (var unit in _compileUnits)
            {
                using var writer = new StringWriter();
                provider.GenerateCodeFromCompileUnit(unit.Value, writer, options);
                result[unit.Key] = writer.ToString();
            }

            return result;
        }

        private void ProcessSchema(Schema schema, string @namespace)
        {
            switch (schema)
            {
                case RecordSchema recordSchema:
                    GenerateRecordClasses(recordSchema, @namespace);
                    break;
                case EnumSchema enumSchema:
                    GenerateEnumeration(enumSchema, @namespace);
                    break;
                case FixedSchema fixedSchema:
                    GenerateFixed(fixedSchema, @namespace);
                    break;
                case ArraySchema arraySchema:
                    ProcessSchema(arraySchema.ItemSchema, @namespace);
                    break;
                case MapSchema mapSchema:
                    ProcessSchema(mapSchema.ValueSchema, @namespace);
                    break;
                case UnionSchema unionSchema:
                    foreach (var unionType in unionSchema.Schemas)
                    {
                        ProcessSchema(unionType, @namespace);
                    }
                    break;
            }
        }

        private void GenerateEnumeration(EnumSchema schema, string @namespace)
        {
            var enumName = GetClassName(schema);
            var fullName = $"{@namespace}.{enumName}";

            if (_processedTypes.Contains(fullName))
                return;

            _processedTypes.Add(fullName);

            var codeNamespace = new CodeNamespace(@namespace);
            var codeUnit = new CodeCompileUnit();
            codeUnit.Namespaces.Add(codeNamespace);

            var enumDeclaration = new CodeTypeDeclaration(enumName)
            {
                IsEnum = true,
                TypeAttributes = System.Reflection.TypeAttributes.Public
            };

            // Add enum values
            foreach (var symbol in schema.Symbols)
            {
                var field = new CodeMemberField(typeof(int), symbol);
                enumDeclaration.Members.Add(field);
            }

            codeNamespace.Types.Add(enumDeclaration);
            _compileUnits[$"{enumName}.cs"] = codeUnit;
        }

        private void GenerateRecordClasses(Schema schema, string @namespace)
        {
            if (schema is RecordSchema recordSchema)
            {
                var className = GetClassName(schema);
                var fullName = $"{@namespace}.{className}";

                if (_processedTypes.Contains(fullName))
                    return;

                _processedTypes.Add(fullName);

                var codeNamespace = new CodeNamespace(@namespace);
                var codeUnit = new CodeCompileUnit();
                codeUnit.Namespaces.Add(codeNamespace);

                // Add required imports
                codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
                codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                codeNamespace.Imports.Add(new CodeNamespaceImport("Avro"));
                codeNamespace.Imports.Add(new CodeNamespaceImport("Avro.Specific"));

                var codeType = new CodeTypeDeclaration(className)
                {
                    IsClass = true,
                    TypeAttributes = System.Reflection.TypeAttributes.Public
                };

                // Add ISpecificRecord interface
                codeType.BaseTypes.Add(new CodeTypeReference(typeof(ISpecificRecord)));

                // Add schema field
                var schemaField = new CodeMemberField(typeof(Schema), "_SCHEMA")
                {
                    Attributes = MemberAttributes.Private | MemberAttributes.Static | MemberAttributes.Final
                };
                schemaField.InitExpression = new CodeSnippetExpression($"Schema.Parse(\"{schema.ToString().Replace("\"", "\\\"")}\")");;
                codeType.Members.Add(schemaField);

                // Add Schema property
                var schemaProp = new CodeMemberProperty
                {
                    Name = "Schema",
                    Type = new CodeTypeReference(typeof(Schema)),
                    Attributes = MemberAttributes.Public | MemberAttributes.Final
                };
                schemaProp.GetStatements.Add(new CodeMethodReturnStatement(
                    new CodeFieldReferenceExpression(null, "_SCHEMA")));
                codeType.Members.Add(schemaProp);

                // Add Get method
                var getMethod = new CodeMemberMethod
                {
                    Name = "Get",
                    ReturnType = new CodeTypeReference(typeof(object)),
                    Attributes = MemberAttributes.Public | MemberAttributes.Final
                };
                getMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "fieldPos"));

                var fieldPosRef = new CodeArgumentReferenceExpression("fieldPos");
                CodeStatement currentStatement = new CodeThrowExceptionStatement(
                    new CodeObjectCreateExpression(
                        typeof(AvroRuntimeException),
                        new CodePrimitiveExpression("Invalid field position")));

                // Build if-else chain from end
                for (int i = recordSchema.Fields.Count - 1; i >= 0; i--)
                {
                    var field = recordSchema.Fields[i];
                    currentStatement = new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            fieldPosRef,
                            CodeBinaryOperatorType.ValueEquality,
                            new CodePrimitiveExpression(i)),
                        new CodeStatement[]
                        {
                            new CodeMethodReturnStatement(
                                new CodePropertyReferenceExpression(
                                    new CodeThisReferenceExpression(),
                                    field.Name))
                        },
                        new CodeStatement[] { currentStatement });
                }

                getMethod.Statements.Add(currentStatement);
                codeType.Members.Add(getMethod);

                // Add Put method
                var putMethod = new CodeMemberMethod
                {
                    Name = "Put",
                    ReturnType = new CodeTypeReference(typeof(void)),
                    Attributes = MemberAttributes.Public | MemberAttributes.Final
                };
                putMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "fieldPos"));
                putMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "fieldValue"));

                var fieldValueRef = new CodeArgumentReferenceExpression("fieldValue");
                CodeStatement currentPutStatement = new CodeThrowExceptionStatement(
                    new CodeObjectCreateExpression(
                        typeof(AvroRuntimeException),
                        new CodePrimitiveExpression("Invalid field position")));

                // Build if-else chain from end
                for (int i = recordSchema.Fields.Count - 1; i >= 0; i--)
                {
                    var field = recordSchema.Fields[i];
                    currentPutStatement = new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            fieldPosRef,
                            CodeBinaryOperatorType.ValueEquality,
                            new CodePrimitiveExpression(i)),
                        new CodeStatement[]
                        {
                            new CodeAssignStatement(
                                new CodePropertyReferenceExpression(
                                    new CodeThisReferenceExpression(),
                                    field.Name),
                                new CodeCastExpression(
                                    GetCodeTypeReference(field.Schema),
                                    fieldValueRef)),
                            new CodeMethodReturnStatement()
                        },
                        new CodeStatement[] { currentPutStatement });
                }

                putMethod.Statements.Add(currentPutStatement);
                codeType.Members.Add(putMethod);

                // Add fields and properties
                foreach (var field in recordSchema.Fields)
                {
                    var propertyType = GetCodeTypeReference(field.Schema);
                    var property = new CodeMemberProperty
                    {
                        Name = field.Name,
                        Type = propertyType,
                        Attributes = MemberAttributes.Public | MemberAttributes.Final
                    };

                    var backingField = new CodeMemberField(propertyType, $"_{field.Name}")
                    {
                        Attributes = MemberAttributes.Private
                    };

                    property.GetStatements.Add(new CodeMethodReturnStatement(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            backingField.Name)));

                    property.SetStatements.Add(new CodeAssignStatement(
                        new CodeFieldReferenceExpression(
                            new CodeThisReferenceExpression(),
                            backingField.Name),
                        new CodePropertySetValueReferenceExpression()));

                    codeType.Members.Add(backingField);
                    codeType.Members.Add(property);

                    // Process nested types
                    ProcessSchema(field.Schema, @namespace);
                }

                codeNamespace.Types.Add(codeType);
                _compileUnits[$"{className}.cs"] = codeUnit;
            }
        }

        private void GenerateFixed(FixedSchema schema, string @namespace)
        {
            var className = GetClassName(schema);
            var fullName = $"{@namespace}.{className}";

            if (_processedTypes.Contains(fullName))
                return;

            _processedTypes.Add(fullName);

            var codeNamespace = new CodeNamespace(@namespace);
            var codeUnit = new CodeCompileUnit();
            codeUnit.Namespaces.Add(codeNamespace);

            // Add required imports
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Runtime.InteropServices"));

            var classDeclaration = new CodeTypeDeclaration(className)
            {
                IsClass = true,
                TypeAttributes = System.Reflection.TypeAttributes.Public
            };

            // Add StructLayout attribute for fixed size
            var structLayoutAttribute = new CodeAttributeDeclaration(
                new CodeTypeReference(typeof(StructLayoutAttribute)),
                new CodeAttributeArgument(
                    new CodeFieldReferenceExpression(
                        new CodeTypeReferenceExpression(typeof(LayoutKind)),
                        "Sequential")));
            classDeclaration.CustomAttributes.Add(structLayoutAttribute);

            // Add size constant
            var sizeConst = new CodeMemberField(typeof(int), "SIZE")
            {
                Attributes = MemberAttributes.Public | MemberAttributes.Const,
                InitExpression = new CodePrimitiveExpression(schema.Size)
            };
            classDeclaration.Members.Add(sizeConst);

            // Add fixed size field
            var field = new CodeMemberField(
                new CodeTypeReference(typeof(byte[])),
                "Value")
            {
                Attributes = MemberAttributes.Public
            };

            // Add initialization in constructor
            var ctor = new CodeConstructor
            {
                Attributes = MemberAttributes.Public
            };
            ctor.Statements.Add(
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(),
                        "Value"),
                    new CodeArrayCreateExpression(
                        typeof(byte),
                        new CodeFieldReferenceExpression(null, "SIZE"))));

            classDeclaration.Members.Add(field);
            classDeclaration.Members.Add(ctor);

            codeNamespace.Types.Add(classDeclaration);
            _compileUnits[$"{className}.cs"] = codeUnit;
        }

        private string GetClassName(Schema schema)
        {
            string fullName = schema.Name;

            if (schema is RecordSchema recordSchema)
            {
                fullName = recordSchema.Name;
            }
            else if (schema is EnumSchema enumSchema)
            {
                fullName = enumSchema.Name;
            }
            else if (schema is FixedSchema fixedSchema)
            {
                fullName = fixedSchema.Name;
            }

            var lastDot = fullName.LastIndexOf('.');
            if (lastDot > 0)
            {
                return fullName.Substring(lastDot + 1);
            }
            return fullName ?? "GeneratedClass";
        }

        private string GetNamespace(Schema schema)
        {
            string? fullName = null;
            string? defaultNamespace = null;

            if (schema is RecordSchema recordSchema)
            {
                fullName = recordSchema.Name;
                defaultNamespace = recordSchema.Namespace;
            }
            else if (schema is EnumSchema enumSchema)
            {
                fullName = enumSchema.Name;
                defaultNamespace = enumSchema.Namespace;
            }
            else if (schema is FixedSchema fixedSchema)
            {
                fullName = fixedSchema.Name;
                defaultNamespace = fixedSchema.Namespace;
            }

            if (fullName != null)
            {
                var lastDot = fullName.LastIndexOf('.');
                if (lastDot > 0)
                {
                    return fullName.Substring(0, lastDot);
                }
            }

            return defaultNamespace ?? "Generated";
        }

        private CodeTypeReference GetCodeTypeReference(Schema schema)
        {
            if (schema is LogicalSchema logicalSchema)
            {
                var logicalType = logicalSchema.LogicalType.Name.ToLowerInvariant();
                return logicalType switch
                {
                    "timestamp-millis" => new CodeTypeReference(typeof(DateTime)),
                    "timestamp-micros" => new CodeTypeReference(typeof(DateTime)),
                    "time-millis" => new CodeTypeReference(typeof(TimeSpan)),
                    "time-micros" => new CodeTypeReference(typeof(TimeSpan)),
                    "date" => new CodeTypeReference(typeof(DateTime)),
                    "decimal" => new CodeTypeReference(typeof(decimal)),
                    "uuid" => new CodeTypeReference(typeof(Guid)),
                    "duration" => new CodeTypeReference(typeof(TimeSpan)),
                    _ => GetCodeTypeReference(logicalSchema.BaseSchema)
                };
            }

            return schema.Tag switch
            {
                Schema.Type.Null => new CodeTypeReference(typeof(object)),
                Schema.Type.String => new CodeTypeReference(typeof(string)),
                Schema.Type.Int => new CodeTypeReference(typeof(int)),
                Schema.Type.Long => new CodeTypeReference(typeof(long)),
                Schema.Type.Float => new CodeTypeReference(typeof(float)),
                Schema.Type.Double => new CodeTypeReference(typeof(double)),
                Schema.Type.Boolean => new CodeTypeReference(typeof(bool)),
                Schema.Type.Bytes => new CodeTypeReference(typeof(byte[])),
                Schema.Type.Fixed => new CodeTypeReference(GetClassName(schema)),
                Schema.Type.Array => new CodeTypeReference(
                    typeof(List<>).FullName,
                    GetCodeTypeReference(((ArraySchema)schema).ItemSchema)),
                Schema.Type.Map => new CodeTypeReference(
                    typeof(Dictionary<,>).FullName,
                    new CodeTypeReference(typeof(string)),
                    GetCodeTypeReference(((MapSchema)schema).ValueSchema)),
                Schema.Type.Record => new CodeTypeReference(GetClassName(schema)),
                Schema.Type.Enumeration => new CodeTypeReference(GetClassName(schema)),
                Schema.Type.Union => GetCodeTypeReferenceForUnion(schema),
                Schema.Type.Logical => GetCodeTypeReference(((LogicalSchema)schema).BaseSchema),
                _ => throw new NotSupportedException($"Schema type {schema.Tag} is not supported")
            };
        }

        private CodeTypeReference GetCodeTypeReferenceForUnion(Schema schema)
        {
            var unionSchema = (UnionSchema)schema;
            var nullableType = unionSchema.Schemas.FirstOrDefault(s => s.Tag != Schema.Type.Null);

            if (nullableType != null)
            {
                var typeReference = GetCodeTypeReference(nullableType);
                if (typeReference.BaseType == "System.String")
                {
                    return typeReference;
                }
                return new CodeTypeReference(typeof(Nullable<>).FullName,
                    new[] { typeReference });
            }

            throw new NotSupportedException("Union must contain a non-null type");
        }
    }
}
