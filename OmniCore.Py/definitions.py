from enum import IntEnum
import os
import logging
from logging.handlers import MemoryHandler

DATA_PATH = "./data/"

KEY_FILE = "key"
LAST_ACTIVATED_FILE = "lastactivated"

POD_FILE = "pod"
POD_FILE_SUFFIX = ".json"

POD_DB_SUFFIX = ".db"
LOGFILE_SUFFIX = ".log"

OMNIPY_LOGGER = "OMNIPY"
OMNIPY_LOGFILE = "omnipy"

OMNIPY_PACKET_LOGGER = "OMNIPACKET"
OMNIPY_PACKET_LOGFILE = "packet"

OMNIPY_DATABASE = "omni.db"

API_VERSION_MAJOR = 1
API_VERSION_MINOR = 3
API_VERSION_REVISION = 6
API_VERSION_BUILD = 19170

REST_URL_PING = "/omnipy/ping"
REST_URL_OMNIPY_SHUTDOWN = "/omnipy/shutdown"
REST_URL_OMNIPY_RESTART = "/omnipy/restart"
REST_URL_OMNIPY_UPDATE = "/omnipy/updatesw"
REST_URL_OMNIPY_WIFI = "/omnipy/updatewlan"
REST_URL_OMNIPY_CHANGE_PASSWORD = "/omnipy/changepw"

REST_URL_TOKEN = "/omnipy/token"
REST_URL_CHECK_PASSWORD = "/omnipy/pwcheck"

REST_URL_NEW_POD = "/omnipy/newpod"
REST_URL_SET_POD_PARAMETERS = "/omnipy/parameters"
REST_URL_GET_PDM_ADDRESS = "/omnipy/pdmspy"

REST_URL_RL_INFO = "/rl/info"

REST_URL_SILENCE_ALARMS = "/pdm/silence"
REST_URL_ARCHIVE_POD = "/pdm/archive"
REST_URL_ACTIVATE_POD = "/pdm/activate"
REST_URL_START_POD = "/pdm/start"
REST_URL_STATUS = "/pdm/status"
REST_URL_PDM_BUSY = "/pdm/isbusy"
REST_URL_ACK_ALERTS = "/pdm/ack"
REST_URL_DEACTIVATE_POD = "/pdm/deactivate"
REST_URL_BOLUS = "/pdm/bolus"
REST_URL_CANCEL_BOLUS = "/pdm/cancelbolus"
REST_URL_SET_TEMP_BASAL = "/pdm/settempbasal"
REST_URL_CANCEL_TEMP_BASAL = "/pdm/canceltempbasal"
REST_URL_SET_BASAL_SCHEDULE = "/pdm/setbasalschedule"

logger = None
packet_logger = None


def ensure_log_dir():
    if not os.path.isdir(DATA_PATH):
        os.mkdir(DATA_PATH)


def getLogger(with_console=False):
    global logger

    if logger is None:
        ensure_log_dir()
        logger = logging.getLogger(OMNIPY_LOGGER)
        logger.setLevel(logging.DEBUG)
        formatter = logging.Formatter('%(asctime)s - %(levelname)s - %(message)s')

        fh = logging.FileHandler(DATA_PATH + OMNIPY_LOGFILE + LOGFILE_SUFFIX)
        fh.setLevel(logging.DEBUG)
        fh.setFormatter(formatter)
        logger.addHandler(fh)

        if with_console:
            ch = logging.StreamHandler()
            ch.setLevel(logging.DEBUG)
            ch.setFormatter(formatter)
            logger.addHandler(ch)

    return logger


def get_packet_logger(with_console=False):
    global packet_logger

    if packet_logger is None:
        ensure_log_dir()
        packet_logger = logging.getLogger(OMNIPY_PACKET_LOGGER)
        packet_logger.setLevel(logging.INFO)
        formatter = logging.Formatter('%(asctime)s %(message)s')

        fh = logging.FileHandler(DATA_PATH + OMNIPY_PACKET_LOGFILE + LOGFILE_SUFFIX)
        fh.setLevel(logging.INFO)
        fh.setFormatter(formatter)
        packet_logger.addHandler(fh)

        if with_console:
            ch = logging.StreamHandler()
            ch.setLevel(logging.DEBUG)
            ch.setFormatter(formatter)
            packet_logger.addHandler(ch)

    return packet_logger


def configureLogging():
    pass


class RequestType(IntEnum):
    pass


class BolusState(IntEnum):
    NotRunning = 0
    Extended = 1
    Immediate = 2


class BasalState(IntEnum):
    NotRunning = 0
    TempBasal = 1
    Program = 2


class PodProgress(IntEnum):
    InitialState = 0
    TankPowerActivated = 1
    TankFillCompleted = 2
    PairingSuccess = 3
    Purging = 4
    ReadyForInjection = 5
    BasalScheduleSet = 6
    Inserting = 7
    Running = 8
    RunningLow = 9
    ErrorShuttingDown = 13
    AlertExpiredShuttingDown = 14
    Inactive = 15


class AlertConfiguration:
    def __init__(self):
        self.alert_index = None
        self.activate = False
        self.trigger_auto_off = False
        self.alert_after_minutes = None
        self.alert_after_reservoir = None
        self.alert_duration = None
        self.beep_type = 0
        self.beep_repeat_type = 0

class BeepPattern(IntEnum):
    Once = 0
    OnceEveryMinuteForThreeMinutesAndRepeatHourly = 1
    OnceEveryMinuteForFifteenMinutes = 2
    OnceEveryMinuteForThreeMinutesAndRepeatEveryFifteenMinutes = 3
    OnceEveryThreeMinutes = 4
    OnceEveryHour = 5
    OnceEveryFifteenMinutes = 6
    OnceEveryQuarterHour = 7
    OnceEveryFiveMinutes = 8


class BeepType(IntEnum):
    NoSound = 0
    BeepFourTimes = 1
    BipBeepFourTimes = 2
    BipBip = 3
    Beep = 4
    BeepThreeTimes = 5
    Beeeep = 6
    BipBipBipTwice = 7
    BeeeepTwice = 8
