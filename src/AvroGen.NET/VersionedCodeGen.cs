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
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                    }
                }

                foreach (CodeTypeDeclaration type in ns.Types)
                {
                    string path = Path.Combine(dir, CodeGenUtil.Instance.UnMangle(type.Name) + ".cs");
                    
                    // Добавляем комментарий с версией схемы
                    var versionComment = new CodeCommentStatement($"Generated from Avro schema version: {_schemaVersion}");
                    type.Comments.Insert(0, versionComment);

                    // Создаем новый пространственный объект для текущего типа
                    var newNamespace = new CodeNamespace(ns.Name);
                    newNamespace.Types.Add(type);
                    
                    using var writer = new StreamWriter(path);
                    cscp.GenerateCodeFromNamespace(newNamespace, writer, opts);
                }
            }
        }
    }
}
