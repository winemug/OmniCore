using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros.Interfaces
{
    public class ErosPodRuntimeVariables
    {
        private int packetSequence = 0;
        public int PacketSequence
        {
            get => packetSequence;
            set
            {
                packetSequence = value % 32;
            }
        }

        public uint? LastNonce { get; set; }
        public int NoncePtr { get; set; }
        public int NonceRuns { get; set; }
        public uint NonceSeed { get; set; }
        public uint? NonceSync { get; set; }
    }
}
