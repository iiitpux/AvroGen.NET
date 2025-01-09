using System.Text.RegularExpressions;

namespace AvroGen.NET
{
    /// <summary>
    /// Класс для проверки версий сгенерированных файлов.
    /// </summary>
    internal class GeneratedFileVersionChecker
    {
        private static readonly Regex VersionRegex = new(@"// Generated from Avro schema version: (\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Проверяет версию сгенерированного файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу</param>
        /// <returns>Версия файла или null, если версия не найдена</returns>
        public static int? GetFileVersion(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var firstLine = File.ReadLines(filePath).FirstOrDefault();
            if (string.IsNullOrEmpty(firstLine))
                return null;

            var match = VersionRegex.Match(firstLine);
            if (!match.Success)
                return null;

            if (int.TryParse(match.Groups[1].Value, out int version))
                return version;

            return null;
        }

        /// <summary>
        /// Проверяет все файлы в директории на соответствие версии.
        /// </summary>
        /// <param name="directory">Директория с файлами</param>
        /// <param name="targetVersion">Целевая версия</param>
        /// <returns>True если все файлы соответствуют версии, иначе False</returns>
        public static bool CheckDirectoryVersion(string directory, int targetVersion)
        {
            //todo- проверка не учитывает разные схемы, а смотрит только на одну схему и версию
            if (!Directory.Exists(directory))
                return false;

            var files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
            if (!files.Any())
                return false;

            foreach (var file in files)
            {
                var version = GetFileVersion(file);
                if (!version.HasValue || version.Value != targetVersion)
                    return false;
            }

            return true;
        }
    }
}
