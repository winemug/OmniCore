using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosPodStatus : IPodStatus
    {
        public uint? Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime Created { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Guid PodId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Faulted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public decimal NotDeliveredInsulin { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public decimal DeliveredInsulin { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public decimal Reservoir { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PodProgress Progress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BasalState BasalState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BolusState BolusState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public uint ActiveMinutes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public byte AlertMask { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private int _message_seq = 0;
        public int MessageSequence
        {
            get => _message_seq;
            set
            {
                _message_seq = value % 16;
            }
        }
    }
}
