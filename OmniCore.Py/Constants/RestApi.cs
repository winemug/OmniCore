namespace OmniCore.Py
{
    public static class RestApi
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
    }
}
