using System;
using System.IO;
using NUnit.Framework; 

namespace Nin.Test.Smoke
{
    public static class TestSetup
    {
        public static string GetDataPath(string filename)
        {
            return GetFullPath("data", filename);
        }

        private static string GetFullPath(string subDirectory, string filename)
        {
            var path = FindDirectoryInTree(subDirectory);
            return Path.Combine(path, filename);
        }

        private static string FindDirectoryInTree(string relativePath)
        {
            var testDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return FindDirectory(testDirectory, relativePath);
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
    }
}