using System;
using System.IO;

namespace OmniCore.Scratch
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Scratch world!");

            var di = new DirectoryInfo(@"D:\omnipod\ti_captures\");
            foreach (var fi in di.GetFiles("*.psd"))
            {
                SnifferUtils.ExtractPacketsFromTISniffer(fi.FullName);
            }
            Console.ReadKey();
        }
    }
}
