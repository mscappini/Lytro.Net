using System;
using System.IO;

namespace Lytro.Splitter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                WL("Usage: lytrosplitter file[.lfp] [... fileN[.lfp]]");
                return;
            }

            foreach (string arg in args)
            {
                try
                {
                    FileInfo fi = new FileInfo(arg);

                    if (!fi.Exists)
                    {
                        throw new FileNotFoundException("Could not find file", arg);
                    }

                    string dir = Directory.GetParent(arg).FullName;

                    LytroFile lfp = new LytroFile(arg);
                    lfp.Load();
                    lfp.Export(dir);
                }
                catch (Exception ex)
                {
                    WL(ex.Message);
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void WL(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
        }

        static void WL(string message)
        {
            Console.WriteLine(message);
        }
    }
}