using Avro;
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
            Directory.CreateDirectory(outputDirectory);

            var cscp = new CSharpCodeProvider();
            var opts = new CodeGeneratorOptions
            {
                BracingStyle = "C",
                IndentString = "\t",
                BlankLinesBetweenMembers = false
            };

            foreach (CodeNamespace ns in CompileUnit.Namespaces)
            {
                string dir = outputDirectory;
                if (!skipDirectories)
                {
                    foreach (string name in CodeGenUtil.Instance.UnMangle(ns.Name).Split('.'))
                    {
                        dir = Path.Combine(dir, name);
                        Directory.CreateDirectory(dir);
                    }
                }

                var newNs = new CodeNamespace(ns.Name);
                newNs.Comments.Add(CodeGenUtil.Instance.FileComment);
                
                foreach (CodeNamespaceImport nci in CodeGenUtil.Instance.NamespaceImports)
                {
                    newNs.Imports.Add(nci);
                }

                foreach (CodeTypeDeclaration ctd in ns.Types)
                {
                    if (ctd.IsClass)
                    {
                        // Добавляем комментарий с версией схемы
                        ctd.Comments.Add(new CodeCommentStatement($"Schema version: {_schemaVersion}"));
                    }
                    
                    string fileName = CodeGenUtil.Instance.UnMangle(ctd.Name) + ".cs";
                    string file = skipDirectories ? Path.Combine(outputDirectory, fileName) : Path.Combine(dir, fileName);
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
