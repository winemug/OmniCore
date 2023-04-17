using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Parts;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Framework.Omnipod.Messages
{

    public struct MessageConstructor
    {
        public Func<IMessageData, IMessageParts> AsParts { get; set; }
        public Func<IMessageParts, IMessageData> AsMessageData { get; set; }
        public PodMessagePartType MainType { get; set; }
        public PodMessagePartType? SubType { get; set; }
    }

    public static class MessageBuilder
    {
        public static List<MessageConstructor> MessageConstructors = new List<MessageConstructor>()
        {
            new MessageConstructor { MainType = PodMessagePartType.RequestBeepConfig, Constructor = () => { return new BeepMessage(); } },
            new MessageConstructor { MainType = PodMessagePartType.RequestCancelDelivery, Constructor = () => { return new CancelMessage(); } },
        };

        //public static IMessage<T> Build(T messageData) where T : IMessageData
        //{
        //    Assembly.GetAssembly(typeof(MessageBuilder)).GetTypes()
        //}

        //public static void GeteGude(MessageData messageData)
        //{
        //    if (messageData is BeepMessageData arr)
        //    {
        //        _ = new BeepMessage().WithMessageData(arr);
        //    }
        //}

        //public static void GetGid(MessageParts parts)
        //{
        //    var mainType = parts.MainPart.Type;
        //    var subType = parts.SubPart?.Type;

        //    switch (mainType)
        //    {
        //        case PodMessagePartType.RequestBeepConfig:
        //            break;
        //        case PodMessagePartType.RequestCancelDelivery:
        //            break;
        //        case PodMessagePartType.ResponseStatus:
        //            break;
        //    }
        //}

        //public void dtt()
        //{
        //    var b = new BeepMessage()
        //        .WithAddress(0x3333333)
        //        .WithSequence(0x14)
        //        .WithMessageData(new BeepMessageData
        //        {
        //            BeepNow = BeepType.Beep
        //        });

        //    var rnddata= new Bytes(new byte[]{ 0x33, 0x54, 0x43 });

        //}

    }
}
