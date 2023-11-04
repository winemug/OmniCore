// using OmniCore.Framework.Omnipod.Parts;
//
// namespace OmniCore.Framework.Omnipod.Requests;
//
// public class SetDeliveryVerificationMessage : PodMessage<RequestSetDeliveryFlags>
// {
//     public byte VerificationFlag0 { get; set; }
//     public byte VerificationFlag1 { get; set; }
//
//     public static Predicate<IMessageParts> CanParse =>
//         parts => parts.MainPart.Type == PodMessagePartType.RequestSetDeliveryFlags;
//
//     public IMessageData FromParts(IMessageParts parts)
//     {
//         VerificationFlag0 = parts.MainPart.Data[4];
//         VerificationFlag1 = parts.MainPart.Data[5];
//         return this;
//     }
//
//     public IMessageParts ToParts()
//     {
//         return new MessageParts(
//             new MessagePart
//             {
//                 Type = PodMessagePartType.RequestSetDeliveryFlags,
//                 RequiresNonce = true,
//                 Data = new Bytes(new[] { VerificationFlag0, VerificationFlag1 })
//             });
//     }
// }