using System;
using System.IO;

namespace Nin.Configuration
{
    public class FileLocator
    {
        public static string FindDirectoryInTree(string relativePath)
        {
            var testDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return FindDirectory(testDirectory, relativePath);
        }

        /// <summary>
        /// Tries to locate a file.  Mainly for easier bulid server config.  Flatten directories and bin\debug\x64 etc.
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static string FindFileInTree(string relativePath)
        {
            var testDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return FindFile(testDirectory, relativePath);
        }

        private static string FindDirectory(string basePath, string subDir)
        {
            while (true)
            {
                if (basePath == null) throw new Exception($"Fant ikke underkatalog '{subDir}'.");
                var candidatePath = Path.Combine(basePath, subDir);
                if (Directory.Exists(candidatePath)) return candidatePath;

                basePath = Path.GetDirectoryName(basePath);
            }
        }

        public static string FindFile(string basePath, string relativeFilePath)
        {
            string filenameOnly = Path.GetFileName(relativeFilePath);
            while (true)
            {
                if (basePath == null) throw new Exception($"Fant ikke fil '{relativeFilePath}' i {basePath} eller kataloger høyere i treet.");
                var candidatePath = Path.Combine(basePath, relativeFilePath);
                if (File.Exists(candidatePath)) 
                    return candidatePath;
                candidatePath = Path.Combine(basePath, filenameOnly);
                if (File.Exists(candidatePath))
                    return candidatePath;

                basePath = Path.GetDirectoryName(basePath);
            }
        }
    }
}