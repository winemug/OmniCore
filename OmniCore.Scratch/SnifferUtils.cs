using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OmniCore.Scratch
{
    public static class SnifferUtils
    {
        public static void ExtractPacketsFromTISniffer(string path)
        {
            using (var sr = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var br = new BinaryReader(sr);
                var streamLength = sr.Length;
                while (sr.Position < streamLength)
                {
                    var packetInformation = br.ReadByte();
                    var packetNumber = br.ReadUInt32();
                    var timestamp = br.ReadUInt64();
                    var packetLength = br.ReadUInt16();
                    var packetData = br.ReadBytes(2051);
                }
            }
        }
    }
}
