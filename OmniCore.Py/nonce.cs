using OmniCore.py;

namespace OmniCore.Py
{

	public class Nonce
    {
        public static uint FAKE_NONCE = 0xD012FA62;
        private int lot;
        private int tid;
        private uint? lastNonce;
        private int seed;
        private int? ptr;
        private int nonce_runs;
        private uint[] table;

		public Nonce(int lot, int tid, uint? seekNonce = null, int seed = 0)
        {
            this.lot = lot;
            this.tid = tid;
            this.lastNonce = null;
            this.seed = seed;
            this.ptr = null;
            this.nonce_runs = 0;
            this._initialize();
			if (seekNonce != null)
            {
                while (this.lastNonce != seekNonce.Value)
                {
                    getNext(true);
                }
            }
        }

		public uint getNext(bool seeking = false)
        {
            if (!seeking && this.nonce_runs > 25)
            {
                this.lastNonce = FAKE_NONCE;
                return FAKE_NONCE;
            }
            var nonce = this.table[this.ptr.Value];
            this.table[this.ptr.Value] = this._generate();
            this.ptr = (int)(nonce & 0xF) + 2;
            this.lastNonce = nonce;
            this.nonce_runs += 1;
            return nonce;
        }

        public void reset()
        {
            this.nonce_runs = 255;
        }
        

	    public void sync(uint syncWord, int msgSequence)
        {
            var w_sum = (this.lastNonce & 0xFFFF) + (CrcUtil.crc16_table[msgSequence] & 0xFFFF)
                        + (this.lot & 0xFFFF) + (this.tid & 0xFFFF);
            this.seed = (int)((w_sum & 0xFFFF) ^ syncWord) & 0xff;
            this.lastNonce = null;
            this.nonce_runs = 0;
            this._initialize();
        }

        public uint _generate()
        {
            this.table[0] = ((this.table[0] >> 16) + (this.table[0] & 0xFFFF) * 0x5D7F) & 0xFFFFFFFF;
            this.table[1] = ((this.table[1] >> 16) + (this.table[1] & 0xFFFF) * 0x8CA0) & 0xFFFFFFFF;
            return (this.table[1] + (this.table[0] << 16)) & 0xFFFFFFFF;
        }

        public void _initialize()
        {
            this.table = new uint[18];
            this.table[0] = (((uint)this.lot & 0xFFFF) + 0x55543DC3 + ((uint)this.lot >> 16) + ((uint)this.seed & 0xFF)) & 0xFFFFFFFF;
            this.table[1] = (((uint)this.tid & 0xFFFF) + 0xAAAAE44E + ((uint)this.tid >> 16) + ((uint)this.seed >> 8)) & 0xFFFFFFFF;

            for (int i = 2; i < 18; i++)
                this.table[i] = this._generate();

            this.ptr = (int)((this.table[0] + this.table[1]) & 0xF) + 2;
            this.lastNonce = null;
        }
    }
}
