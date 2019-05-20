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
            this.Pod.nonce_syncword = null;

            if (pod.nonce_last.HasValue)
            {
                var nonce_ptr = this.Initialize(this.Pod.id_lot.Value, this.Pod.id_t.Value, this.Pod.nonce_seed);
                int nonce_runs = 0;
                uint nonce_generated = pod.nonce_last.Value ^ FAKE_NONCE;
                while (nonce_generated != pod.nonce_last)
                {
                    nonce_generated = GetNextInternal(ref nonce_ptr);
                    nonce_runs++;
                }
                this.Pod.nonce_runs = nonce_runs;
                this.Pod.nonce_ptr = nonce_ptr;
            }
            else
            {
                this.Pod.nonce_runs = 0;
                this.Pod.nonce_seed = 0;
                this.Pod.nonce_ptr = this.Initialize(this.Pod.id_lot.Value, this.Pod.id_t.Value, 0);
            }
        }

        public uint GetNext()
        {
            if (this.Pod.nonce_runs++ > 25)
                this.Pod.nonce_last = FAKE_NONCE;
            else
            {
                var ptr = this.Pod.nonce_ptr;
                this.Pod.nonce_last = GetNextInternal(ref ptr);
                this.Pod.nonce_ptr = ptr;
            }
            return this.Pod.nonce_last.Value;
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
            this.Pod.nonce_runs = 32;
        }

	    public void Sync(int msgSequence)
        {
            var w_sum = (this.Pod.nonce_last & 0xFFFF) + (CrcUtil.Crc16Table[msgSequence] & 0xFFFF)
                        + (this.Pod.id_lot.Value & 0xFFFF) + (this.Pod.id_t.Value & 0xFFFF);
            this.Pod.nonce_seed = (uint)((w_sum & 0xFFFF) ^ this.Pod.nonce_syncword) & 0xff;
            this.Pod.nonce_runs = 0;
            this.Pod.nonce_syncword = null;
            this.Initialize(this.Pod.id_lot.Value, this.Pod.id_t.Value, this.Pod.nonce_seed);
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
