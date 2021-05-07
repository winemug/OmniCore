using System;
using System.IO;
using System.Linq;

namespace OmniCore.Scratch
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Scratch world!");

            foreach (var fi in new DirectoryInfo(@"C:\test\").GetFiles("*.omni").OrderBy(x => x.Name))
            {
                Console.Write(fi.Name);
                SnifferUtils.ExtractPacketsFromOpenOmniRTL(fi.FullName, @"C:\test\result.txt");
            }
            //Console.ReadKey();
        }
    }
}
