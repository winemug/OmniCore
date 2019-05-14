from .crc import crc16_table

FAKE_NONCE = 0xD012FA62


class Nonce:
    def __init__(self, lot, tid, seekNonce = None, seed = 0):
        self.lot = lot
        self.tid = tid
        self.lastNonce = None
        self.seed = seed
        self.ptr = None
        self.nonce_runs = 0
        self._initialize()
        if seekNonce is not None:
            while self.lastNonce != seekNonce:
                self.getNext(True)

    def getNext(self, seeking = False):
        if not seeking and self.nonce_runs > 200:
            self.lastNonce = FAKE_NONCE
            return FAKE_NONCE
        nonce = self.table[self.ptr]
        self.table[self.ptr] = self._generate()
        self.ptr = (nonce & 0xF) + 2
        self.lastNonce = nonce
        self.nonce_runs += 1
        return nonce

    def reset(self):
        self.nonce_runs = 255

    def sync(self, syncWord, msgSequence):
        w_sum = (self.lastNonce & 0xFFFF) + (crc16_table[msgSequence] & 0xFFFF) \
              + (self.lot & 0xFFFF) + (self.tid & 0xFFFF)
        self.seed = ((w_sum & 0xFFFF) ^ syncWord) & 0xff
        self.lastNonce = None
        self.nonce_runs = 0
        self._initialize()

    def _generate(self):
        self.table[0] = ((self.table[0] >> 16) + (self.table[0] & 0xFFFF) * 0x5D7F) & 0xFFFFFFFF
        self.table[1] = ((self.table[1] >> 16) + (self.table[1] & 0xFFFF) * 0x8CA0) & 0xFFFFFFFF
        return (self.table[1] + (self.table[0] << 16)) & 0xFFFFFFFF

    def _initialize(self):
        self.table = [0]*18
        self.table[0] = ((self.lot & 0xFFFF) + 0x55543DC3 + (self.lot >> 16) + (self.seed & 0xFF)) & 0xFFFFFFFF
        self.table[1] = ((self.tid & 0xFFFF) + 0xAAAAE44E + (self.tid >> 16) + (self.seed >> 8)) & 0xFFFFFFFF

        for i in range(2, 18):
            self.table[i] = self._generate()

        self.ptr = ((self.table[0] + self.table[1]) & 0xF) + 2
        self.lastNonce = None
