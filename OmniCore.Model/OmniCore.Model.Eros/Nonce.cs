using OmniCore.Model.Eros.Data;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Utilities;

namespace OmniCore.Model.Eros
{

	public class Nonce
    {
        public static uint FAKE_NONCE = 0xD012FA62;
        private uint[] table;

        private ErosPod Pod;

        public Nonce(ErosPod pod)
        {
            Pod = pod;
            Pod.RuntimeVariables.NonceSync = null;

            if (pod.RuntimeVariables.LastNonce.HasValue)
            {
                var nonce_ptr = this.Initialize(this.Pod.Lot.Value, this.Pod.Serial.Value, Pod.RuntimeVariables.NonceSeed);
                int nonce_runs = 0;
                uint nonce_generated = Pod.RuntimeVariables.LastNonce.Value ^ FAKE_NONCE;
                while (nonce_generated != Pod.RuntimeVariables.LastNonce)
                {
                    nonce_generated = GetNextInternal(ref nonce_ptr);
                    nonce_runs++;
                }
                Pod.RuntimeVariables.NonceRuns = nonce_runs;
                Pod.RuntimeVariables.NoncePtr = nonce_ptr;
            }
            else
            {
                Pod.RuntimeVariables.NonceRuns = 0;
                Pod.RuntimeVariables.NonceSeed = 0;
                Pod.RuntimeVariables.NoncePtr = this.Initialize(this.Pod.Lot.Value, this.Pod.Serial.Value, 0);
            }
        }

        public uint GetNext()
        {
            if (Pod.RuntimeVariables.NonceRuns++ > 25)
                Pod.RuntimeVariables.LastNonce = FAKE_NONCE;
            else
            {
                var ptr = Pod.RuntimeVariables.NoncePtr;
                Pod.RuntimeVariables.LastNonce = GetNextInternal(ref ptr);
                Pod.RuntimeVariables.NoncePtr = ptr;
            }
            return Pod.RuntimeVariables.LastNonce.Value;
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
            Pod.RuntimeVariables.NonceRuns = 32;
        }

	    public void Sync(int msgSequence)
        {
            var w_sum = (Pod.RuntimeVariables.LastNonce & 0xFFFF) + (CrcUtil.Crc16Table[msgSequence] & 0xFFFF)
                        + (this.Pod.Lot.Value & 0xFFFF) + (this.Pod.Serial.Value & 0xFFFF);
            Pod.RuntimeVariables.NonceSeed = (uint)((w_sum & 0xFFFF) ^ Pod.RuntimeVariables.NonceSync) & 0xff;
            Pod.RuntimeVariables.NonceRuns = 0;
            Pod.RuntimeVariables.NonceSync = null;
            Pod.RuntimeVariables.NoncePtr = this.Initialize(this.Pod.Lot.Value, this.Pod.Serial.Value, Pod.RuntimeVariables.NonceSeed);
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
