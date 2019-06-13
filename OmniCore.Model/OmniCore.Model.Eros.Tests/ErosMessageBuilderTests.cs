using Microsoft.VisualStudio.TestTools.UnitTesting;
using OmniCore.Model.Enums;
using OmniCore.Model.Utilities;
using System;

namespace OmniCore.Model.Eros.Tests
{
    [TestClass]
    public class ErosMessageBuilderTests
    {
        [TestMethod]
        public void TestBasalSchedule_1()
        {
            var emb = new ErosMessageBuilder();
            var schedule = new decimal[48];
            for (int i = 0; i < 48; i++)
                schedule[i] = 1.15m;

            emb.WithBasalSchedule(schedule, 1, 15, 20);
            var message = emb.Build();
            var eMessage = message as ErosMessage;
            Assert.IsNotNull(eMessage);
            Assert.AreEqual(message.RequestType, RequestType.SetBasalSchedule);
            Assert.AreEqual(eMessage.parts.Count, 2);

            var part0 = FromHexString("0002ca021b800005f80bf80bf80b");
            var part1 = FromHexString("0000146f003512bf159000eed54d");

            Assert.IsTrue(eMessage.parts[0].PartData.CompareTo(part0) == 0);
            Assert.IsTrue(eMessage.parts[1].PartData.CompareTo(part1) == 0);
        }

        [TestMethod]
        public void TestBasalSchedule_2()
        {
            var emb = new ErosMessageBuilder();
            var schedule = new decimal[48];
            for (int i = 0; i < 48; i++)
                schedule[i] = 0.05m;

            emb.WithBasalSchedule(schedule, 20, 59, 55);
            var message = emb.Build();
            var eMessage = message as ErosMessage;
            Assert.IsNotNull(eMessage);
            Assert.AreEqual(message.RequestType, RequestType.SetBasalSchedule);
            Assert.AreEqual(eMessage.parts.Count, 2);

            var part0 = FromHexString("0000692900280000f800f800f800");
            var part1 = FromHexString("0000001e004c4b4000f015752a00");

            Assert.IsTrue(eMessage.parts[0].PartData.CompareTo(part0) == 0);
            Assert.IsTrue(eMessage.parts[1].PartData.CompareTo(part1) == 0);
        }

        private Bytes FromHexString(string hexstr)
        {
            var b = new Bytes();
            for (int i = 0; i < hexstr.Length; i += 2)
            {
                b.Append(Convert.ToByte(hexstr.Substring(i, 2), 16));
            }
            return b;
        }
    }
}
