/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Avro;
using Avro.Specific;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace AvroGen.NET
{
    /// <summary>
    /// Расширенный генератор кода с поддержкой версионирования схем
    /// </summary>
    public class VersionedCodeGen : CodeGen
    {
        private readonly int _schemaVersion;

        /// <summary>
        /// Создает новый экземпляр генератора кода с версией схемы
        /// </summary>
        /// <param name="schemaVersion">Версия схемы</param>
        public VersionedCodeGen(int schemaVersion)
        {
            _schemaVersion = schemaVersion;
        }

        /// <summary>
        /// Записывает сгенерированные типы в файлы с добавлением информации о версии схемы
        /// </summary>
        /// <param name="outputDirectory">Директория для сохранения файлов</param>
        /// <param name="skipDirectories">Если true, то файлы будут генерироваться без создания поддиректорий для namespace</param>
        public override void WriteTypes(string outputDirectory, bool skipDirectories = false)
        {
            // Создаем директорию, если её нет
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Создаем атрибут версии схемы, если его еще нет
            var attributeCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public class SchemaVersionAttribute : Attribute
{
    public int Version { get; }
    public SchemaVersionAttribute(int version)
    {
        Version = version;
    }
}";
            var attributeFile = Path.Combine(outputDirectory, "SchemaVersionAttribute.cs");
            if (!File.Exists(attributeFile))
            {
                File.WriteAllText(attributeFile, attributeCode);
            }

            // Для каждого типа в CompileUnit
            foreach (CodeNamespace ns in CompileUnit.Namespaces)
            {
                string targetDirectory = outputDirectory;
                if (!skipDirectories && !string.IsNullOrEmpty(ns.Name))
                {
                    // Создаем поддиректории для namespace только если не установлен флаг skipDirectories
                    targetDirectory = Path.Combine(outputDirectory, ns.Name.Replace('.', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(targetDirectory);
                }

                foreach (CodeTypeDeclaration type in ns.Types)
                {
                    // Добавляем атрибут версии схемы
                    if (type.IsClass && !type.Name.Equals("SchemaVersionAttribute"))
                    {
                        type.CustomAttributes.Add(
                            new CodeAttributeDeclaration(
                                new CodeTypeReference("SchemaVersion"),
                                new CodeAttributeArgument[] {
                                    new CodeAttributeArgument(
                                        new CodePrimitiveExpression(_schemaVersion)
                                    )
                                }
                            )
                        );
                    }

                    // Генерируем код с атрибутом
                    using (var writer = new StreamWriter(Path.Combine(targetDirectory, type.Name + ".cs")))
                    {
                        var provider = new Microsoft.CSharp.CSharpCodeProvider();
                        var options = new CodeGeneratorOptions
                        {
                            BracingStyle = "C",
                            BlankLinesBetweenMembers = true
                        };

                        var unit = new CodeCompileUnit();
                        var newNs = new CodeNamespace(ns.Name);
                        // Добавляем необходимые using'и
                        newNs.Imports.Add(new CodeNamespaceImport("System"));
                        newNs.Imports.Add(new CodeNamespaceImport("Avro.Specific"));
                        
                        newNs.Types.Add(type);
                        unit.Namespaces.Add(newNs);

                        provider.GenerateCodeFromCompileUnit(unit, writer, options);
                    }
                }
            }
        }
    }
}
