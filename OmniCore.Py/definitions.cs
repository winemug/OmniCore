using System;

namespace OmniCore.Py
{
    public static class definitions
    {

        public static int API_VERSION_MAJOR = 1;
        public static int API_VERSION_MINOR = 3;
        public static int API_VERSION_REVISION = 6;
        public static int API_VERSION_BUILD = 19170;

        public static string REST_URL_PING = "/omnipy/ping";
        public static string REST_URL_OMNIPY_SHUTDOWN = "/omnipy/shutdown";
        public static string REST_URL_OMNIPY_RESTART = "/omnipy/restart";
        public static string REST_URL_OMNIPY_UPDATE = "/omnipy/updatesw";
        public static string REST_URL_OMNIPY_WIFI = "/omnipy/updatewlan";
        public static string REST_URL_OMNIPY_CHANGE_PASSWORD = "/omnipy/changepw";

        public static string REST_URL_TOKEN = "/omnipy/token";
        public static string REST_URL_CHECK_PASSWORD = "/omnipy/pwcheck";

        public static string REST_URL_NEW_POD = "/omnipy/newpod";
        public static string REST_URL_SET_POD_PARAMETERS = "/omnipy/parameters";
        public static string REST_URL_GET_PDM_ADDRESS = "/omnipy/pdmspy";

        public static string REST_URL_RL_INFO = "/rl/info";

        public static string REST_URL_SILENCE_ALARMS = "/pdm/silence";
        public static string REST_URL_ARCHIVE_POD = "/pdm/archive";
        public static string REST_URL_ACTIVATE_POD = "/pdm/activate";
        public static string REST_URL_START_POD = "/pdm/start";
        public static string REST_URL_STATUS = "/pdm/status";
        public static string REST_URL_PDM_BUSY = "/pdm/isbusy";
        public static string REST_URL_ACK_ALERTS = "/pdm/ack";
        public static string REST_URL_DEACTIVATE_POD = "/pdm/deactivate";
        public static string REST_URL_BOLUS = "/pdm/bolus";
        public static string REST_URL_CANCEL_BOLUS = "/pdm/cancelbolus";
        public static string REST_URL_SET_TEMP_BASAL = "/pdm/settempbasal";
        public static string REST_URL_CANCEL_TEMP_BASAL = "/pdm/canceltempbasal";
        public static string REST_URL_SET_BASAL_SCHEDULE = "/pdm/setbasalschedule";

        static logger py_logger = new logger();
        static logger pk_logger = new logger();

        public static logger getLogger()
        {
            return py_logger;
        }

        public static logger get_packet_logger()
        {
            return pk_logger;
        }
    }

    public class logger
    {
        //TODO: all logger methods used
        public void log(string text)
        {
            //TODO: where do we log?
        }

        public void exception(string text, Exception e)
        {
            //TODO:
        }
    }

    public enum BolusState
    {
        NotRunning = 0,
        Extended = 1,
        Immediate = 2
    }


    public enum BasalState
    {
        NotRunning = 0,
        TempBasal = 1,
        Program = 2
    }


    public enum PodProgress
    {
        InitialState = 0,
        TankPowerActivated = 1,
        TankFillCompleted = 2,
        PairingSuccess = 3,
        Purging = 4,
        ReadyForInjection = 5,
        BasalScheduleSet = 6,
        Inserting = 7,
        Running = 8,
        RunningLow = 9,
        ErrorShuttingDown = 13,
        AlertExpiredShuttingDown = 14,
        Inactive = 15
    }


    public class AlertConfiguration
    {
        public int? alert_index = null;
        public bool activate = false;
        public bool trigger_auto_off = false;
        public int? alert_after_minutes = null;
        public decimal? alert_after_reservoir = null;
        public int alert_duration = 0;
        public BeepType beep_type = BeepType.NoSound;
        public BeepPattern beep_repeat_type = BeepPattern.Once;
    }

    public enum BeepPattern
    {
        Once = 0,
        OnceEveryMinuteForThreeMinutesAndRepeatHourly = 1,
        OnceEveryMinuteForFifteenMinutes = 2,
        OnceEveryMinuteForThreeMinutesAndRepeatEveryFifteenMinutes = 3,
        OnceEveryThreeMinutes = 4,
        OnceEveryHour = 5,
        OnceEveryFifteenMinutes = 6,
        OnceEveryQuarterHour = 7,
        OnceEveryFiveMinutes = 8
    }


    public enum BeepType
    {
        NoSound = 0,
        BeepFourTimes = 1,
        BipBeepFourTimes = 2,
        BipBip = 3,
        Beep = 4,
        BeepThreeTimes = 5,
        Beeeep = 6,
        BipBipBipTwice = 7,
        BeeeepTwice = 8
    }
}
