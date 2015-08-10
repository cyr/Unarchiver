using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common;
using SharpCompress.Reader;
using SharpCompress.Reader.Rar;

namespace Unarchiver
{
    public class Archive
    {
        public readonly List<FileInfo> Files = new List<FileInfo>();

        public Archive(string name)
        {
            Name = name;
        }

        public void AddFile(FileInfo file)
        {
            if (file.Extension == ".sfv")
            {
                SfvFile = file;
                return;
            }

            Files.Add(file);
            
            if (IsFirstVolume(file))
                First = file;
        }

        public FileInfo SfvFile { get; set; }

        private bool IsFirstVolume(FileInfo file)
        {
            var fileName = file.Name.ToLower();

            if (fileName.Contains(".part"))
                return fileName.EndsWith(".part01.rar");

            if (First != null && fileName.EndsWith(".r00"))
                return true;

            return fileName.EndsWith(".rar");
        }

        private IEnumerable<Stream> OpenFiles()
        {
            var orderedFiles = Files.OrderBy(OrderMethod).ToArray();

            return orderedFiles.Select(f => f.OpenRead());
        }

        private int OrderMethod(FileInfo file)
        {
            var name = file.Name;
            int orderMethod;

            if (GetOrder(name, ".part", out orderMethod))
                return orderMethod;

            if (GetOrder(name, ".r", out orderMethod))
                return orderMethod;

            return -1;
        }

        private static bool GetOrder(string name, string pattern, out int order)
        {
            if (name.Contains(pattern))
            {
                int value;
                if (Int32.TryParse(name.Substring(name.LastIndexOf(pattern, StringComparison.Ordinal) + pattern.Length, 2), out value))
                {
                    order = value;
                    return true;
                }
            }
            order = -1;
            return false;
        }

        private Stream OpenFile()
        {
            return Files.First().OpenRead();
        }

        public IReader CreateReader()
        {
            if (Files.Count == 1)
                return ReaderFactory.Open(OpenFile(), Options.None);

            return RarReader.Open(OpenFiles(), Options.None);
        }

        public FileInfo First { get; set; }
        public string Name { get; }
    }
}