using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SharpCompress.Common;
using SharpCompress.Reader;

namespace Unarchiver
{
    public class Unarchiver
    {
        private readonly Regex _rarPattern = new Regex(@"^(.*)\.(?:rar|r\d\d|part\d\d\.rar)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private DirectoryInfo _basePath;

        public IEnumerable<string> Unarchive(string path)
        {
            _basePath = new DirectoryInfo(path);

            if (!_basePath.Exists)
                throw new ArgumentException("Path does not exist.");

            var folders = GetFolders(_basePath);

            return folders
                .SelectMany(GetArchives).AsParallel()
                .SelectMany(ExtractFiles)
                .ToArray();
        }

        private static List<DirectoryInfo> GetFolders(DirectoryInfo di)
        {
            var folders = new List<DirectoryInfo> {di};
            folders.AddRange(di.GetDirectories("*", SearchOption.AllDirectories));
            return folders;
        }

        private IEnumerable<Archive> GetArchives(DirectoryInfo dir)
        {
            Console.WriteLine("Searching for archives in {0}", dir.FullName);

            var archives = new Dictionary<string, Archive>();
            
            foreach (var file in dir.GetFiles("*", SearchOption.TopDirectoryOnly))
            {
                var match = _rarPattern.Match(file.Name);

                if (!match.Success)
                    continue;

                string name = GetArchiveName(file.Name);

                AddFileToArchive(archives, name, file);
            }

            return archives.Values;
        }

        private static void AddFileToArchive(Dictionary<string, Archive> archives, string name, FileInfo file)
        {
            Archive archive;

            if (!archives.TryGetValue(name, out archive))
            {
                archive = new Archive(name);
                archives[name] = archive;
            }

            archive.AddFile(file);
        }

        private static string GetArchiveName(string name)
        {
            if (name.Contains(".part"))
                return name.Substring(0, name.LastIndexOf(".part"));

            return name.Substring(0, name.LastIndexOf(".r"));
        }

        private IEnumerable<string> ExtractFiles(Archive archive)
        {
            Console.WriteLine("Found archive: {0}", archive.Name);

            var createdEntries = new List<string>();
            try
            {
                var reader = archive.CreateReader();

                while (reader.MoveToNextEntry())
                {
                    reader.WriteEntryToDirectory(_basePath.FullName, ExtractOptions.Overwrite | ExtractOptions.ExtractFullPath);

                    createdEntries.Add(reader.Entry.Key);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem extracting from {0}: {1}", archive.Name, e);
            }

            return createdEntries;
        }
    }
}