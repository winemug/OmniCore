// using OmniCore.Framework.Omnipod.Parts;
// using OmniCore.Shared.Entities;
// using OmniCore.Shared.Enums;
//
// namespace OmniCore.Framework.Omnipod.Responses;
//
// public class StatusMessage : PodMessage<ResponseStatus>
// {
//     public PodStatusModel StatusModel { get; set; }
//     public PodProgressModel ProgressModel { get; set; }
//
//     public static Predicate<IMessageParts> CanParse =>
//         parts => parts.MainPart.Type == PodMessagePartType.ResponseStatus;
//
//     public IMessageData FromParts(IMessageParts parts)
//     {
//         var data = parts.MainPart.Data;
//         var b0 = data[0];
//         var d0 = data.DWord(1);
//         var d1 = data.DWord(5);
//
//         ProgressModel = new PodProgressModel
//         {
//             Progress = (PodProgress)(b0 & 0x0F),
//             Faulted = (d1 & 0x80000000) != 0
//         };
//         StatusModel = new PodStatusModel
//         {
//             ExtendedBolusActive = (b0 & 0b10000000) > 0,
//             ImmediateBolusActive = (b0 & 0b01000000) > 0,
//             TempBasalActive = (b0 & 0b00100000) > 0,
//             BasalActive = (b0 & 0b00010000) > 0,
//
//             PulsesDelivered = (d0 >> 15) & 0b000001111111111111,
//             LastProgrammingCommandSequence = (d0 >> 11) & 0b00001111,
//             PulsesPending = d0 & 0b0000011111111111,
//
//             UnackedAlertsMask = (d1 >> 23) & 0x0F,
//             ActiveMinutes = (d1 >> 10) & 0b0001111111111111,
//             PulsesRemaining = d1 & 0b0000001111111111
//         };
//         return this;
//     }
//
//     public IMessageParts ToParts()
//     {
//         throw new NotImplementedException();
//     }
// }