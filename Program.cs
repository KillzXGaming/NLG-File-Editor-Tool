using NextLevelLibrary.LM2;
using Toolbox.Core.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileConverter
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine($"No arguments given! Drag/drop a dict to dump!");
                Console.Read();
                return;
            }

            if (Directory.Exists(args[0]))
            {
                //ensure dict is present to inject
                string name = new DirectoryInfo(args[0]).Name;
                string path = $"{name}.dict";

                if (!File.Exists(path))
                {
                    Console.WriteLine($"No dictionary file found! Expected {path}");
                    return;
                }

                var data = DictionaryFile.ReadDictionaryData(path);
                string folder = Path.GetFileNameWithoutExtension(path);

                InjectConfigs(data, folder);

                data.Save(name + ".NEW.data");
                Console.WriteLine($"Finished saving new dict file!");
            }
            else if (File.Exists(args[0])) //dump
            {
                //ensure dict is used
                if (!args[0].EndsWith(".dict"))
                {
                    Console.WriteLine($"No dictionary file given! Drag/drop a .dict to dump!");
                    return;
                }

                var data = DictionaryFile.ReadDictionaryData(args[0]);
                string folder = Path.GetFileNameWithoutExtension(args[0]);

                data.Extract(folder);

                DumpConfigs(data, folder);
                Console.WriteLine($"Finished dumping files!");
            }
            else
            {
                Console.WriteLine($"No dictionary file given! Drag/drop a .dict to dump!");
            }
            Console.Read();
        }

        static void DumpConfigs(DataFile data, string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (var file in data.Table.Files)
            {
                if (file.FileType == NextLevelLibrary.ChunkFileType.Config)
                    File.WriteAllBytes(Path.Combine(folder, $"{file.Hash}.cfg"), file.Data.ToArray());
            }
        }

        static void InjectConfigs(DataFile data, string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            foreach (var file in Directory.GetFiles(folder))
            {
                uint.TryParse(Path.GetFileNameWithoutExtension(file), out uint hash);
                if (hash == 0)
                    continue;

                //set the target file
                foreach (var f in data.Table.Files)
                {
                    if (f.Hash == hash && f.FileType == NextLevelLibrary.ChunkFileType.Config)
                        f.Data = new MemoryStream(File.ReadAllBytes(file));
                }
            }
        }
    }
}