using OmniCore.Model.Enumerations;

namespace OmniCore.Model.Entities
{
    public class PodResponseStatus
    {
        public bool Faulted { get; set; }
        public BasalState BasalState { get; set; }
        public BolusState BolusState { get; set; }
        public int NotDelivered { get; set; }
        public int Delivered { get; set; }
        public int Reservoir { get; set; }
        public int MessageSequence { get; set; }
        public int ActiveMinutes { get; set; }
        public byte AlertMask { get; set; }
    }
}