using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Protocol.Base
{
    public class NonceGenerator
    {
        private uint Lot;
        private uint Tid;
        private uint? Nonce;
        private int? Seed;

        private uint[] nonceTable = new uint[18];

        public NonceGenerator(uint lot, uint tid, uint? nonce, int? seed)
        {
            this.Lot = lot;
            this.Tid = tid;
            this.Nonce = nonce;
            this.Seed = seed;
            this.Initialize();
        }
        public uint GetNext()
        {
            throw new NotImplementedException();
        }

        private void Initialize()
        {
        }
    }
}
