using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace OmniCore.Scratch
{
    public static class SnifferUtils
    {
        public static void ExtractPacketsFromTISniffer(string path)
        {
            //using (var sr = new FileStream(path, FileMode.Open, FileAccess.Read))
            //{
            //    var br = new BinaryReader(sr);
            //    var streamLength = sr.Length;
            //    while (sr.Position < streamLength)
            //    {
            //        var packetInformation = br.ReadByte();
            //        var packetNumber = br.ReadUInt32();
            //        var timestamp = br.ReadUInt64();
            //        var packetLength = br.ReadUInt16();
            //        var packetStatus = br.ReadBytes(1);
            //        var packetData = br.ReadBytes(2050);

            //        var decoded = ManchesterCodec.Decode(packetData);
            //        if (decoded.Length > 0)
            //        {
            //            var crc = CrcUtil.Crc8(decoded, decoded.Length - 1);
            //            if (decoded[decoded.Length - 1] == crc)
            //            {
            //                Console.WriteLine(BitConverter.ToString(decoded).Replace("-", ""));
            //            }
            //        }
            //    }

            //}
        }

        public static void ExtractPacketsFromOpenOmniRTL(string source, string dest)
        {
            using (var sw = new StreamWriter(dest, true))
            using (var sr = new StreamReader(source))
            {
                var r = new Regex(@"(.+) ID1:(\w+) PTYPE:(\w\w\w) SEQ:(\d\d) (?:ID2:(\w+) )?(?:CON:(\w+) )?(?:B9:(\w\w) BLEN:.+ BODY:(.+) )?CRC\:");
                DateTime? vdLast = null;
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var m = r.Match(line);
                    if (m.Success)
                    {
                        var datestr = m.Groups[1].Value;
                        var id1 = m.Groups[2].Value;
                        var ptype = m.Groups[3].Value;
                        var seq = m.Groups[4].Value;
                        var id2 = m.Groups[5].Value;
                        var con = m.Groups[6].Value;
                        var b9 = m.Groups[7].Value;
                        var body = m.Groups[8].Value;

                        if (id1 != "ffffffff" && id1 != "1f0e89f1")
                            continue;

                        var vdx = DateTime.Parse(datestr);
                        double lx = 0;
                        if (vdLast.HasValue)
                        {
                            var ts = (vdx - vdLast.Value).TotalSeconds;
                            if (ts > 1.0)
                            {
                                sw.WriteLine($"-- --- -- - -------- -------- Silence {ts:0.000} seconds");
                            }
                        }
                        vdLast = vdx;

                        var vseq = Int32.Parse(seq);

                        if (!string.IsNullOrEmpty(b9))
                        {
                            var vb9 = Convert.ToInt32(b9, 16);
                            var mseq = (vb9 >> 2) & 0x0F;
                            var bb9 = (vb9 >> 6);
                            sw.WriteLine($"{vseq:X2} {ptype} {mseq:X2} {bb9:X1} {id1:X8} {id2:X8} {vdx} {body}");
                        }
                        else if (!string.IsNullOrEmpty(id2))
                        {
                            sw.WriteLine($"{vseq:X2} {ptype} ** * {id1:X8} {id2:X8} {vdx}");
                        }
                        else
                        {
                            sw.WriteLine($"{vseq:X2} {ptype} ** * {id1:X8} ******** {vdx} {con}");
                        }
                    }
                }
            }
        }
    }
}
