// using OmniCore.Framework.Omnipod.Parts;
//
// namespace OmniCore.Framework.Omnipod.Requests;
//
// public class DeactivateMessage : PodMessage<RequestDeactivatePod>
// {
//     public static Predicate<IMessageParts> CanParse =>
//         parts => parts.MainPart.Type == PodMessagePartType.RequestDeactivatePod;
//
//     public IMessageData FromParts(IMessageParts parts)
//     {
//         return this;
//     }
//
//     public IMessageParts ToParts()
//     {
//         return new MessageParts(
//             new MessagePart
//             {
//                 Type = PodMessagePartType.RequestDeactivatePod,
//                 RequiresNonce = true,
//                 Data = new Bytes()
//             });
//     }
// }