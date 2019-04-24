import abc
from enum import IntEnum


class TxPower(IntEnum):
    Lowest = 0
    Low = 1
    Normal = 2
    High = 3
    Highest = 4


class PacketRadio(abc.ABC):
    @abc.abstractmethod
    def __init__(self):
        pass

    @abc.abstractmethod
    def connect(self, force_initialize=False):
        pass

    @abc.abstractmethod
    def disconnect(self, ignore_errors=True):
        pass

    @abc.abstractmethod
    def get_info(self):
        pass

    @abc.abstractmethod
    def init_radio(self, force_init=False):
        pass

    @abc.abstractmethod
    def tx_up(self):
        pass

    @abc.abstractmethod
    def tx_down(self):
        pass

    @abc.abstractmethod
    def set_tx_power(self, tx_power):
        pass

    @abc.abstractmethod
    def get_packet(self, timeout=5.0):
        pass

    @abc.abstractmethod
    def send_and_receive_packet(self, packet, repeat_count, delay_ms, timeout_ms, retry_count, preamble_ext_ms):
        pass

    @abc.abstractmethod
    def send_packet(self, packet, repeat_count, delay_ms, preamble_extension_ms):
        pass