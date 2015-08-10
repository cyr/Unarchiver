using System;

namespace Unarchiver.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var unarchiver = new Unarchiver();

            var basePath = string.Join(" ", args);

            var unpackedFiles = unarchiver.Unarchive(basePath);

            foreach (var file in unpackedFiles)
            {
                Console.WriteLine("Extracted file: {0}", file);
            }

            Console.WriteLine();
            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
