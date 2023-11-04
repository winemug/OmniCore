// using OmniCore.Framework.Omnipod.Parts;
// using OmniCore.Shared.Enums;
//
// namespace OmniCore.Framework.Omnipod.Responses;
//
// public class LogDumpMessage : PodMessage<ResponseInfo>
// {
//     public static Predicate<IMessageParts> CanParse =>
//         parts =>
//             parts.MainPart.Type == PodMessagePartType.ResponseInfo &&
//             (parts.MainPart.Data[0] == (byte)PodStatusType.PulseLogRecent ||
//              parts.MainPart.Data[0] == (byte)PodStatusType.PulseLogPrevious ||
//              parts.MainPart.Data[0] == (byte)PodStatusType.PulseLogLast
//             );
//
//     public IMessageData FromParts(IMessageParts parts)
//     {
//         var data = parts.MainPart.Data;
//         return this;
//     }
//
//     public IMessageParts ToParts()
//     {
//         throw new NotImplementedException();
//     }
//
//     private void ProcessLogEntry(uint v)
//     {
//         var encoderCount = v >> 26;
//         var pumpDrive = (0x01000000 & v) >> 24;
//         var el = v & 0x0000003F;
//         var encoderComputed = 0;
//         if ((el & 0x20) > 0)
//             encoderComputed = (int)(0xFFFFFFE0 | v);
//         else
//             encoderComputed = (int)el;
//
//         var loadCount = (v >> 7) & 0x1ff;
//         loadCount = loadCount | ((v >> 16) & 0x01);
//     }
// }