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
        public void BasalSchedule1()
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
        public void BasalSchedule2()
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
            
        [TestMethod]
        public void BasalSchedule3()
        {
            var emb = new ErosMessageBuilder();
            var schedule = new decimal[]
            {
                2.75m, 2.75m, 2.75m,
                1.25m, 1.25m, 1.25m,
                1.75m, 1.75m, 1.75m,
                0.05m, 0.05m, 0.05m,
                0.35m, 0.35m, 0.35m,
                1.95m, 1.95m, 1.95m,
                1.05m, 1.05m, 1.05m,
                0.05m, 0.05m, 0.05m,
                1.65m, 1.65m, 1.65m,
                0.85m, 0.85m, 0.85m,
                14.95m, 14.95m, 14.95m,
                30.00m, 30.00m, 30.00m,
                0.15m, 0.15m, 0.15m,
                1.05m, 1.05m, 1.05m,
                6.95m, 6.95m, 6.95m,
                1.00m, 1.00m, 1.00m
            };

            emb.WithBasalSchedule(schedule, 14, 35, 25);
            var message = emb.Build();
            var eMessage = message as ErosMessage;
            Assert.IsNotNull(eMessage);
            Assert.AreEqual(message.RequestType, RequestType.SetBasalSchedule);
            Assert.AreEqual(eMessage.parts.Count, 2);

            var part0 = FromHexString("0005231d2e180007281b000d180c281100011800280300141813280a000118002810000918082895212c00021801280a00461845200a");
            var part1 = FromHexString("0009004500d2ee2003390063e02e017700dbba00020d009cf292000f15752a0000690310bcdb0249008cd9b1013b01059449000f15752a0001ef00a675a200ff01432096118500125f2d2328000927c0002d07270e00013b010594490825002784e8012c0112a880");

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
