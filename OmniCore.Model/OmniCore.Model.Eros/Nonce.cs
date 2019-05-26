using OmniCore.Model.Utilities;

namespace OmniCore.Model.Eros
{

	public class Nonce
    {
        public static uint FAKE_NONCE = 0xD012FA62;
        private uint[] table;

        public Pod Pod { get; private set; }

        public Nonce(Pod pod)
        {
            this.Pod = pod;
            this.Pod.NonceSync = null;

            if (pod.LastNonce.HasValue)
            {
                var nonce_ptr = this.Initialize(this.Pod.Lot.Value, this.Pod.Serial.Value, this.Pod.NonceSeed);
                int nonce_runs = 0;
                uint nonce_generated = pod.LastNonce.Value ^ FAKE_NONCE;
                while (nonce_generated != pod.LastNonce)
                {
                    nonce_generated = GetNextInternal(ref nonce_ptr);
                    nonce_runs++;
                }
                this.Pod.NonceRuns = nonce_runs;
                this.Pod.NoncePtr = nonce_ptr;
            }
            else
            {
                this.Pod.NonceRuns = 0;
                this.Pod.NonceSeed = 0;
                this.Pod.NoncePtr = this.Initialize(this.Pod.Lot.Value, this.Pod.Serial.Value, 0);
            }
        }

        public uint GetNext()
        {
            if (this.Pod.NonceRuns++ > 25)
                this.Pod.LastNonce = FAKE_NONCE;
            else
            {
                var ptr = this.Pod.NoncePtr;
                this.Pod.LastNonce = GetNextInternal(ref ptr);
                this.Pod.NoncePtr = ptr;
            }
            return this.Pod.LastNonce.Value;
        }

        private uint GetNextInternal(ref int nonce_ptr)
        {
            var nonce_value = this.table[nonce_ptr];
            this.table[nonce_ptr] = Shuffle();
            nonce_ptr = (int)(nonce_value & 0xF) + 2;
            return nonce_value;
        }

        public void Reset()
        {
            this.Pod.NonceRuns = 32;
        }

	    public void Sync(int msgSequence)
        {
            var w_sum = (this.Pod.LastNonce & 0xFFFF) + (CrcUtil.Crc16Table[msgSequence] & 0xFFFF)
                        + (this.Pod.Lot.Value & 0xFFFF) + (this.Pod.Serial.Value & 0xFFFF);
            this.Pod.NonceSeed = (uint)((w_sum & 0xFFFF) ^ this.Pod.NonceSync) & 0xff;
            this.Pod.NonceRuns = 0;
            this.Pod.NonceSync = null;
            this.Initialize(this.Pod.Lot.Value, this.Pod.Serial.Value, this.Pod.NonceSeed);
        }

        private uint Shuffle()
        {
            this.table[0] = ((this.table[0] >> 16) + (this.table[0] & 0xFFFF) * 0x5D7F) & 0xFFFFFFFF;
            this.table[1] = ((this.table[1] >> 16) + (this.table[1] & 0xFFFF) * 0x8CA0) & 0xFFFFFFFF;
            return (this.table[1] + (this.table[0] << 16)) & 0xFFFFFFFF;
        }

        private int Initialize(uint lot, uint tid, uint seed)
        {
            this.table = new uint[18];
            this.table[0] = ((lot & 0xFFFF) + 0x55543DC3 + (lot >> 16) + (seed & 0xFF)) & 0xFFFFFFFF;
            this.table[1] = ((tid & 0xFFFF) + 0xAAAAE44E + (tid >> 16) + (seed >> 8)) & 0xFFFFFFFF;

            for (int i = 2; i < 18; i++)
                this.table[i] = Shuffle();

            return (int)((this.table[0] + this.table[1]) & 0xF) + 2;
        }
    }
}
