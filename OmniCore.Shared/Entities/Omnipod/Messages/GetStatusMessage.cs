// using OmniCore.Framework.Omnipod.Parts;
// using OmniCore.Shared;
// using OmniCore.Shared.Enums;
//
// namespace OmniCore.Framework.Omnipod.Requests;
//
// public class GetStatusMessage : PodMessage<RequestStatus>
// {
//     public PodStatusType StatusType { get; set; }
//     public static Predicate<IMessageParts> CanParse => parts => parts.MainPart.Type == PodMessagePartType.RequestStatus;
//
//     public IMessageData FromParts(IMessageParts parts)
//     {
//         StatusType = (PodStatusType)parts.MainPart.Data[0];
//         return this;
//     }
//
//     public IMessageParts ToParts()
//     {
//         return new MessageParts(
//             new MessagePart
//             {
//                 Type = PodMessagePartType.RequestStatus,
//                 RequiresNonce = false,
//                 Data = new Bytes((byte)StatusType)
//             });
//     }
// }