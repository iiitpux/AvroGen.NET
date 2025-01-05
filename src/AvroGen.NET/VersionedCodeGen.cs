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
using Microsoft.CSharp;

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
            var cscp = new CSharpCodeProvider();

            var opts = new CodeGeneratorOptions();
            opts.BracingStyle = "C";
            opts.IndentString = "\t";
            opts.BlankLinesBetweenMembers = false;

            CodeNamespaceCollection nsc = CompileUnit.Namespaces;
            for (int i = 0; i < nsc.Count; i++)
            {
                var ns = nsc[i];

                string dir = outputDirectory;
                if (skipDirectories != true)
                {
                    foreach (string name in CodeGenUtil.Instance.UnMangle(ns.Name).Split('.'))
                    {
                        dir = Path.Combine(dir, name);
                    }
                }
                Directory.CreateDirectory(dir);

                var newNs = new CodeNamespace(ns.Name);
                newNs.Comments.Add(CodeGenUtil.Instance.FileComment);
                
                foreach (CodeNamespaceImport nci in CodeGenUtil.Instance.NamespaceImports)
                {
                    newNs.Imports.Add(nci);
                }

                var types = ns.Types;
                for (int j = 0; j < types.Count; j++)
                {
                    var ctd = types[j];
                    
                    // Добавляем атрибут версии схемы
                    if (ctd.IsClass && !ctd.Name.Equals("SchemaVersionAttribute"))
                    {
                        ctd.CustomAttributes.Add(
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
                    
                    string file = Path.Combine(dir, Path.ChangeExtension(CodeGenUtil.Instance.UnMangle(ctd.Name), "cs"));
                    using (var writer = new StreamWriter(file, false))
                    {
                        newNs.Types.Add(ctd);
                        cscp.GenerateCodeFromNamespace(newNs, writer, opts);
                        newNs.Types.Remove(ctd);
                    }
                }
            }
        }
    }
}
