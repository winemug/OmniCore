#!/usr/bin/python3
from threading import Thread
import signal
import base64
from uuid import getnode as get_mac
from decimal import *
from threading import Lock
from Crypto.Cipher import AES
import simplejson as json
from flask import Flask, request, send_from_directory
from datetime import datetime
import time
from podcomm.pdm import Pdm, PdmLock
from podcomm.pod import Pod
from podcomm.pr_rileylink import RileyLink
from podcomm.definitions import *
from logging import FileHandler


g_oldest_diff = None
g_time_diffs = []
g_key = None
g_pod = None
g_pdm = None
g_deny = False
g_tokens = []
g_token_lock = Lock()

app = Flask(__name__, static_url_path="/")
configureLogging()
logger = getLogger(with_console=True)
get_packet_logger(with_console=True)


class RestApiException(Exception):
    def __init__(self, msg="Unknown"):
        self.error_message = msg

    def __str__(self):
        return self.error_message

def _set_pod(pod):
    global g_pod
    global g_pdm

    g_pod = pod

    g_pod.path = DATA_PATH + POD_FILE + POD_FILE_SUFFIX
    g_pod.path_db = DATA_PATH + POD_FILE + POD_DB_SUFFIX
    g_pod.Save()

    if g_pdm is not None:
        g_pdm.stop_radio()
        g_pdm = None


def _get_pod():
    global g_pod
    try:
        if g_pod is None:
            if os.path.exists(DATA_PATH + POD_FILE + POD_FILE_SUFFIX):
                g_pod = Pod.Load(DATA_PATH + POD_FILE + POD_FILE_SUFFIX, DATA_PATH + POD_FILE + POD_DB_SUFFIX)
            else:
                g_pod = Pod()
                g_pod.path = DATA_PATH + POD_FILE + POD_FILE_SUFFIX
                g_pod.path_db = DATA_PATH + POD_FILE + POD_DB_SUFFIX
                g_pod.Save()
        return g_pod
    except:
        logger.exception("Error while loading pod")
        return None


def _get_pdm():
    global g_pdm
    try:
        if g_pdm is None:
            g_pdm = Pdm(_get_pod())
        return g_pdm
    except:
        logger.exception("Error while creating pdm instance")
        return None


def _flush_handlers(logger):
    for handler in logger.handlers:
        if isinstance(handler, MemoryHandler):
            handler.flush()
        if isinstance(handler, FileHandler):
            handler.flush()
            handler.close()

def _archive_pod():
    global g_pod
    global g_pdm
    try:
        g_pod = None
        g_pdm = None
        archive_name = None
        archive_suffix = datetime.utcnow().strftime("_%Y%m%d_%H%M%S")

        if os.path.isfile(DATA_PATH + POD_FILE + POD_FILE_SUFFIX):
            archive_name = DATA_PATH + POD_FILE + archive_suffix + POD_FILE_SUFFIX
            os.rename(DATA_PATH + POD_FILE + POD_FILE_SUFFIX,
                                     archive_name)
        if os.path.isfile(DATA_PATH + POD_FILE + POD_DB_SUFFIX):
            os.rename(DATA_PATH + POD_FILE + POD_DB_SUFFIX,
                      DATA_PATH + POD_FILE + archive_suffix + POD_DB_SUFFIX)

        _flush_handlers(getLogger())
        _flush_handlers(get_packet_logger())

        if os.path.isfile(DATA_PATH + OMNIPY_PACKET_LOGFILE + LOGFILE_SUFFIX):
            os.rename(DATA_PATH + OMNIPY_PACKET_LOGFILE + LOGFILE_SUFFIX,
                      DATA_PATH + OMNIPY_PACKET_LOGFILE + archive_suffix + LOGFILE_SUFFIX)

        if os.path.isfile(DATA_PATH + OMNIPY_LOGFILE + LOGFILE_SUFFIX):
            os.rename(DATA_PATH + OMNIPY_LOGFILE + LOGFILE_SUFFIX,
                      DATA_PATH + OMNIPY_LOGFILE + archive_suffix + LOGFILE_SUFFIX)

        return archive_name
    except:
        logger.exception("Error while archiving existing pod")


def _get_next_pod_address():
    try:
        try:
            with open(DATA_PATH + LAST_ACTIVATED_FILE, "r") as lastfile:
                addr = int(lastfile.readline(), 16)
                blast = (addr & 0x0000000f) + 1
                addr = (addr & 0xfffffff0) | (blast & 0x0000000f)
        except:
            mac = get_mac()
            b0 = 0x34
            b1 = (mac >> 12) & 0xff
            b2 = (mac >> 4) & 0xff
            b3 = (mac << 4) & 0xf0
            addr = (b0 << 24) | (b1 << 16) | (b2 << 8) | b3
            addr = addr | 0x00000008
        return addr
    except:
        logger.exception("Error while getting next radio address")


def _save_activated_pod_address(addr):
    try:
        with open(DATA_PATH + LAST_ACTIVATED_FILE, "w") as lastfile:
            lastfile.write(hex(addr))
    except:
        logger.exception("Error while storing activated radio address")


def _create_response(success, response, pod_status=None):

    if pod_status is None:
        pod_status = {}
    elif pod_status.__class__ != dict:
        pod_status = pod_status.__dict__

    if response is None:
        response = {}
    elif response.__class__ != dict:
        response = response.__dict__

    return json.dumps({"success": success,
                       "response": response,
                       "status": pod_status,
                       "datetime": time.time(),
                       "api": {"version_major": API_VERSION_MAJOR, "version_minor": API_VERSION_MINOR,
                               "version_revision": API_VERSION_REVISION, "version_build": API_VERSION_BUILD}
                       }, indent=4, sort_keys=True)


def _verify_auth(request_obj):
    global g_deny
    try:
        if g_deny:
            raise RestApiException("Pdm is shutting down")

        i = request_obj.args.get("i")
        a = request_obj.args.get("auth")
        if i is None or a is None:
            raise RestApiException("Authentication failed")

        iv = base64.b64decode(i)
        auth = base64.b64decode(a)

        cipher = AES.new(g_key, AES.MODE_CBC, iv)
        token = cipher.decrypt(auth)

        with g_token_lock:
            if token in g_tokens:
                g_tokens.remove(token)
            else:
                raise RestApiException("Invalid authentication token")
    except RestApiException:
        logger.exception("Authentication error")
        raise
    except Exception:
        logger.exception("Error during verify_auth")
        raise


def _adjust_time(adjustment):
    logger.info("Adjusting local time by %d ms" % adjustment)
    pdm = _get_pdm()
    if pdm is not None:
        pdm.set_time_adjustment(adjustment / 1000)


def _api_result(result_lambda, generic_err_message):
    global g_time_diffs, g_oldest_diff
    try:
        if g_deny:
            raise RestApiException("Pdm is shutting down")

        if request.args.get('req_t') is not None:
            req_time = int(request.args.get('req_t'))
            local_time = int(time.time() * 1000)
            difference_ms = (req_time - local_time)
            if g_oldest_diff is None:
                g_oldest_diff = local_time

            if g_oldest_diff - local_time > 300:
                g_time_diffs = [difference_ms]
                g_oldest_diff = local_time
            else:
                g_time_diffs.append(difference_ms)

            if len(g_time_diffs) > 3:
                diff_avg = sum(g_time_diffs) / len(g_time_diffs)
                g_time_diffs = []

                if diff_avg > 30000 or diff_avg < -30000:
                    _adjust_time(diff_avg)

        return _create_response(True,
                               response=result_lambda(), pod_status=_get_pod())
    except RestApiException as rae:
        return _create_response(False, response=rae, pod_status=_get_pod())
    except Exception as e:
        logger.exception(generic_err_message)
        return _create_response(False, response=e, pod_status=_get_pod())


def _get_pdm_address(timeout):
    packet = None
    with PdmLock():
        try:
            radio = _get_pdm().get_radio()
            radio.stop()
            packet = radio.get_packet(timeout)
        finally:
            radio.disconnect()
            radio.start()

    if packet is None:
        raise RestApiException("No packet received")

    return packet.address


def archive_pod():
    _verify_auth(request)
    pod = Pod()
    _archive_pod()
    _set_pod(pod)

def ping():
    return {"pong": None}


def create_token():
    token = bytes(os.urandom(16))
    with g_token_lock:
        g_tokens.append(token)
    return {"token": base64.b64encode(token)}


def check_password():
    _verify_auth(request)


def get_pdm_address():
    _verify_auth(request)

    timeout = 30000
    if request.args.get('timeout') is not None:
        timeout = int(request.args.get('timeout')) * 1000
        if timeout > 30000:
            raise RestApiException("Timeout cannot be more than 30 seconds")

    address = _get_pdm_address(timeout)

    return {"radio_address": address, "radio_address_hex": "%8X" % address}


def new_pod():
    _verify_auth(request)

    pod = Pod()

    if request.args.get('id_lot') is not None:
        pod.id_lot = int(request.args.get('id_lot'))
    if request.args.get('id_t') is not None:
        pod.id_t = int(request.args.get('id_t'))
    if request.args.get('radio_address') is not None:
        pod.radio_address = int(request.args.get('radio_address'))
    else:
        pod.radio_address = 0

    if pod.radio_address == 0:
        pod.radio_address = _get_pdm_address(45000)

    _archive_pod()
    _set_pod(pod)


def activate_pod():
    _verify_auth(request)

    pod = _get_pod()
    if pod.state_progress >= PodProgress.Running:
        pod = Pod()
        _archive_pod()
        _set_pod(pod)

    pdm = _get_pdm()

    req_address = _get_next_pod_address()
    utc_offset = int(request.args.get('utc'))
    pdm.activate_pod(req_address, utc_offset=utc_offset)
    _save_activated_pod_address(req_address)


def start_pod():
    _verify_auth(request)

    pdm = _get_pdm()

    schedule=[]

    for i in range(0,48):
        rate = Decimal(request.args.get("h"+str(i)))
        schedule.append(rate)

    pdm.inject_and_start(schedule)


def _int_parameter(obj, parameter):
    if request.args.get(parameter) is not None:
        obj.__dict__[parameter] = int(request.args.get(parameter))
        return True
    return False


def _float_parameter(obj, parameter):
    if request.args.get(parameter) is not None:
        obj.__dict__[parameter] = float(request.args.get(parameter))
        return True
    return False


def _bool_parameter(obj, parameter):
    if request.args.get(parameter) is not None:
        val = str(request.args.get(parameter))
        bval = False
        if val == "1" or val.capitalize() == "TRUE":
            bval = True
        obj.__dict__[parameter] = bval
        return True
    return False


def set_pod_parameters():
    _verify_auth(request)

    pod = _get_pod()
    try:
        reset_nonce = False
        if _int_parameter(pod, "id_lot"):
            reset_nonce = True
        if _int_parameter(pod, "id_t"):
            reset_nonce = True

        if reset_nonce:
            pod.nonce_last = None
            pod.nonce_seed = 0

        if _int_parameter(pod, "radio_address"):
            pod.radio_packet_sequence = 0
            pod.radio_message_sequence = 0

        _float_parameter(pod, "var_utc_offset")
        _float_parameter(pod, "var_maximum_bolus")
        _float_parameter(pod, "var_maximum_temp_basal_rate")
        _float_parameter(pod, "var_alert_low_reservoir")
        _int_parameter(pod, "var_alert_replace_pod")
        _bool_parameter(pod, "var_notify_bolus_start")
        _bool_parameter(pod, "var_notify_bolus_cancel")
        _bool_parameter(pod, "var_notify_temp_basal_set")
        _bool_parameter(pod, "var_notify_temp_basal_cancel")
        _bool_parameter(pod, "var_notify_basal_schedule_change")
    except:
        raise
    finally:
        pod.Save()


def get_rl_info():
    _verify_auth(request)
    r = RileyLink()
    return r.get_info()


def get_status():
    _verify_auth(request)
    t = request.args.get('type')
    if t is not None:
        req_type = int(t)
    else:
        req_type = 0

    pdm = _get_pdm()
    id = pdm.update_status(req_type)

    return {"row_id":id}


def deactivate_pod():
    _verify_auth(request)
    pdm = _get_pdm()
    id = pdm.deactivate_pod()
    _archive_pod()
    return {"row_id":id}


def bolus():
    _verify_auth(request)

    pdm = _get_pdm()
    amount = Decimal(request.args.get('amount'))
    id = pdm.bolus(amount)
    return {"row_id":id}


def cancel_bolus():
    _verify_auth(request)

    pdm = _get_pdm()
    id = pdm.cancel_bolus()
    return {"row_id":id}


def set_temp_basal():
    _verify_auth(request)

    pdm = _get_pdm()
    amount = Decimal(request.args.get('amount'))
    hours = Decimal(request.args.get('hours'))
    id = pdm.set_temp_basal(amount, hours, False)
    return {"row_id":id}


def cancel_temp_basal():
    _verify_auth(request)

    pdm = _get_pdm()
    id = pdm.cancel_temp_basal()
    return {"row_id":id}


def set_basal_schedule():
    _verify_auth(request)
    pdm = _get_pdm()

    schedule=[]

    for i in range(0,48):
        rate = Decimal(request.args.get("h"+str(i)))
        schedule.append(rate)

    utc_offset = int(request.args.get("utc"))
    pdm.pod.var_utc_offset = utc_offset

    id = pdm.set_basal_schedule(schedule)
    return {"row_id":id}


def is_pdm_busy():
    pdm = _get_pdm()
    return {"busy": pdm.is_busy()}


def acknowledge_alerts():
    _verify_auth(request)

    mask = Decimal(request.args.get('alertmask'))
    pdm = _get_pdm()
    id = pdm.acknowledge_alerts(mask)
    return {"row_id":id}


def silence_alarms():
    _verify_auth(request)

    pdm = _get_pdm()
    id = pdm.hf_silence_will_fall()
    return {"row_id":id}

def shutdown():
    global g_deny
    _verify_auth(request)

    g_deny = True

    pdm = _get_pdm()
    while pdm.is_busy():
        time.sleep(1)
    os.system("sudo shutdown -h")
    return {"shutdown": time.time()}


def restart():
    global g_deny
    _verify_auth(request)

    g_deny = True

    pdm = _get_pdm()
    while pdm.is_busy():
        time.sleep(1)
    os.system("sudo shutdown -r")
    return {"restart": time.time()}


def update_omnipy():
    global g_deny
    _verify_auth(request)

    g_deny = True
    pdm = _get_pdm()
    while pdm.is_busy():
        time.sleep(1)
    os.system("/bin/bash /home/pi/omnipy/scripts/pi-update.sh")
    return {"update started": time.time()}


def update_wlan():
    global g_deny
    _verify_auth(request)

    ssid = str(request.args.get('ssid'))
    pw = str(request.args.get('pw'))

    g_deny = True
    pdm = _get_pdm()
    while pdm.is_busy():
        time.sleep(1)
    os.system('/bin/bash /home/pi/omnipy/scripts/pi-setwifi.sh "%s" "%s"' % (ssid, pw))
    return {"update started": time.time()}


def update_password():
    global g_key

    _verify_auth(request)

    iv = base64.b64decode(request.args.get("i"))
    pw_enc = base64.b64decode(request.args.get('pw'))

    cipher = AES.new(g_key, AES.MODE_CBC, iv)
    new_key = cipher.decrypt(pw_enc)

    with open(DATA_PATH + KEY_FILE, "wb") as key_file:
        key_file.write(new_key)
    g_key = new_key


@app.route("/")
def main_page():
    try:
        return app.send_static_file("omnipy.html")
    except:
        logger.exception("Error while serving root file")

@app.route('/content/<path:path>')
def send_content(path):
    try:
        return send_from_directory("static", path)
    except:
        logger.exception("Error while serving static file from %s" % path)

@app.route(REST_URL_PING)
def a00():
    return _api_result(lambda: ping(), "Failure while pinging")

@app.route(REST_URL_TOKEN)
def a01():
    return _api_result(lambda: create_token(), "Failure while creating token")

@app.route(REST_URL_CHECK_PASSWORD)
def a02():
    return _api_result(lambda: check_password(), "Failure while verifying password")

@app.route(REST_URL_GET_PDM_ADDRESS)
def a03():
    return _api_result(lambda: get_pdm_address(), "Failure while reading address from PDM")

@app.route(REST_URL_NEW_POD)
def a04():
    return _api_result(lambda: new_pod(), "Failure while creating a new pod")

@app.route(REST_URL_SET_POD_PARAMETERS)
def a05():
    return _api_result(lambda: set_pod_parameters(), "Failure while setting parameters")

@app.route(REST_URL_RL_INFO)
def a06():
    return _api_result(lambda: get_rl_info(), "Failure while getting RL info")

@app.route(REST_URL_STATUS)
def a07():
    return _api_result(lambda: get_status(), "Failure while executing getting pod status")

@app.route(REST_URL_ACK_ALERTS)
def a08():
    return _api_result(lambda: acknowledge_alerts(), "Failure while executing acknowledge alerts")

@app.route(REST_URL_DEACTIVATE_POD)
def a09():
    return _api_result(lambda: deactivate_pod(), "Failure while executing deactivate pod")

@app.route(REST_URL_BOLUS)
def a10():
    return _api_result(lambda: bolus(), "Failure while executing bolus")

@app.route(REST_URL_CANCEL_BOLUS)
def a11():
    return _api_result(lambda: cancel_bolus(), "Failure while executing cancel bolus")

@app.route(REST_URL_SET_TEMP_BASAL)
def a12():
    return _api_result(lambda: set_temp_basal(), "Failure while executing set temp basal")

@app.route(REST_URL_CANCEL_TEMP_BASAL)
def a13():
    return _api_result(lambda: cancel_temp_basal(), "Failure while executing cancel temp basal")

@app.route(REST_URL_PDM_BUSY)
def a14():
    return _api_result(lambda: is_pdm_busy(), "Failure while verifying if pdm is busy")

@app.route(REST_URL_OMNIPY_SHUTDOWN)
def a15():
    return _api_result(lambda: shutdown(), "Failure while executing shutdown")

@app.route(REST_URL_OMNIPY_RESTART)
def a16():
    return _api_result(lambda: restart(), "Failure while executing reboot")

@app.route(REST_URL_ACTIVATE_POD)
def a17():
    return _api_result(lambda: activate_pod(), "Failure while activating a new pod")

@app.route(REST_URL_START_POD)
def a18():
    return _api_result(lambda: start_pod(), "Failure while starting a newly activated pod")

@app.route(REST_URL_SET_BASAL_SCHEDULE)
def a19():
    return _api_result(lambda: set_basal_schedule(), "Failure while setting a basal schedule")

@app.route(REST_URL_ARCHIVE_POD)
def a20():
    return _api_result(lambda: archive_pod(), "Failure while archiving pod")

@app.route(REST_URL_OMNIPY_UPDATE)
def a21():
    return _api_result(lambda: update_omnipy(), "Failure while executing software update")

@app.route(REST_URL_OMNIPY_WIFI)
def a22():
    return _api_result(lambda: update_wlan(), "Failure while updating wifi parameters")

@app.route(REST_URL_OMNIPY_CHANGE_PASSWORD)
def a23():
    return _api_result(lambda: update_password(), "Failure while changing omnipy password")

@app.route(REST_URL_SILENCE_ALARMS)
def a24():
    return _api_result(lambda: silence_alarms(), "Failure while silencing")

def _run_flask():
    try:
        app.run(host='0.0.0.0', port=4444, debug=True, use_reloader=False)
    except:
        logger.exception("Error while running rest api, exiting")


def _exit_with_grace():
    try:
        global g_deny
        g_deny = True
        pdm = _get_pdm()
        while pdm.is_busy():
            time.sleep(5)
        _flush_handlers(getLogger())
        _flush_handlers(get_packet_logger())
    except:
        logger.exception("error during graceful shutdown")

    exit(0)


if __name__ == '__main__':
    logger.info("Rest api is starting")

    try:
        with open(DATA_PATH + KEY_FILE, "rb") as keyfile:
            g_key = keyfile.read(32)
    except IOError:
        logger.exception("Error while reading keyfile. Did you forget to set a password?")
        raise

    try:
        os.system("sudo systemctl restart systemd-timesyncd && sudo systemctl daemon-reload")
    except:
        logger.exception("Error while reloading timesync daemon")

    signal.signal(signal.SIGTERM, _exit_with_grace)

    t = Thread(target=_run_flask)
    t.setDaemon(True)
    t.start()

    try:
        while True:
            time.sleep(1)

    except KeyboardInterrupt:
        _exit_with_grace()

