// using OmniCore.Framework.Omnipod.Parts;
// using OmniCore.Shared.Enums;
//
// namespace OmniCore.Framework.Omnipod.Responses;
//
// public class AlertInfoMessage : PodMessage<ResponseInfo>
// {
//     public ushort[] Alerts = new ushort[8];
//     public ushort Unknown0 { get; set; }
//
//     public static Predicate<IMessageParts> CanParse =>
//         parts =>
//             parts.MainPart.Type == PodMessagePartType.ResponseInfo &&
//             parts.MainPart.Data[0] == (byte)PodStatusType.Alerts;
//
//     public IMessageData FromParts(IMessageParts parts)
//     {
//         var data = parts.MainPart.Data;
//         Unknown0 = data.Word(1);
//         Alerts[0] = data.Word(3);
//         Alerts[1] = data.Word(5);
//         Alerts[2] = data.Word(7);
//         Alerts[3] = data.Word(9);
//         Alerts[4] = data.Word(11);
//         Alerts[5] = data.Word(13);
//         Alerts[6] = data.Word(15);
//         Alerts[7] = data.Word(17);
//         return this;
//     }
//
//     public IMessageParts ToParts()
//     {
//         throw new NotImplementedException();
//     }
// }