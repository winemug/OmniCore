#!/usr/bin/python3

from podcomm.definitions import *
import requests
import simplejson as json
from Crypto.Cipher import AES
import os
import base64
import argparse

ROOT_URL = "http://127.0.0.1:4444"

configureLogging()
logger = getLogger()


def get_auth_params():
    with open(DATA_PATH + KEY_FILE, "rb") as keyfile:
        key = keyfile.read(32)

    r = requests.get(ROOT_URL + REST_URL_TOKEN, timeout=20)
    j = json.loads(r.text)
    token = base64.b64decode(j["response"]["token"])

    i = os.urandom(16)
    cipher = AES.new(key, AES.MODE_CBC, i)
    a = cipher.encrypt(token)
    auth = base64.b64encode(a)
    iv = base64.b64encode(i)

    return {"auth": auth, "i": iv}


def call_api(root, path, pa):
    r = requests.get(root + path, params = pa)
    print(r.text)


def read_pdm_address(args, pa):
    call_api(args.url, REST_URL_GET_PDM_ADDRESS, pa)


def new_pod(args, pa):

    if args.id_lot is not None:
        pa["id_lot"] = args.id_lot
    if args.id_t is not None:
        pa["id_t"] = args.id_t
    if args.radio_address is not None:
        if str(args.radio_address).lower().startswith("0x"):
            pa["radio_address"] = int(args.radio_address[2:], 16)
        else:
            pa["radio_address"] = int(args.radio_address)
    call_api(args.url, REST_URL_NEW_POD, pa)


def temp_basal(args, pa):
    pa["amount"] = args.basalrate
    pa["hours"] = args.hours
    call_api(args.url, REST_URL_SET_TEMP_BASAL, pa)


def cancel_temp_basal(args, pa):
    call_api(args.url, REST_URL_CANCEL_TEMP_BASAL, pa)


def bolus(args, pa):
    pa["amount"] = args.units
    call_api(args.url, REST_URL_BOLUS, pa)


def cancel_bolus(args, pa):
    call_api(args.url, REST_URL_CANCEL_BOLUS, pa)


def status(args, pa):
    pa["type"] = args.req_type
    call_api(args.url, REST_URL_STATUS, pa)


def deactivate(args, pa):
    call_api(args.url, REST_URL_DEACTIVATE_POD, pa)


def activate(args, pa):
    pa["utc"] = args.utcoffset
    call_api(args.url, REST_URL_ACTIVATE_POD, pa)


def archive(args, pa):
    call_api(args.url, REST_URL_ARCHIVE_POD, pa)


def silence(args, pa):
    call_api(args.url, REST_URL_SILENCE_ALARMS, pa)

def start(args, pa):
    for i in range(0,48):
        pa["h" + str(i)] = args.basalrate

    call_api(args.url, REST_URL_START_POD, pa)

def shutdown(args, pa):
    call_api(args.url, REST_URL_OMNIPY_SHUTDOWN, pa)


def restart(args, pa):
    call_api(args.url, REST_URL_OMNIPY_RESTART, pa)


def main():
    parser = argparse.ArgumentParser(description="Send a command to omnipy API")
    parser.add_argument("-u", "--url", type=str, default="http://127.0.0.1:4444", required=False)

    subparsers = parser.add_subparsers(dest="sub_cmd")

    subparser = subparsers.add_parser("readpdm", help="readpdm -h")
    subparser.set_defaults(func=read_pdm_address)

    subparser = subparsers.add_parser("newpod", help="newpod -h")
    subparser.add_argument("id_lot", type=int, help="Lot number of the pod", default=None, nargs="?")
    subparser.add_argument("id_t", type=int, help="Serial number of the pod", default=None, nargs="?")
    subparser.add_argument("radio_address", help="Radio radio_address of the pod", default=None, nargs="?")
    subparser.set_defaults(func=new_pod)

    subparser = subparsers.add_parser("silence", help="silence -h")
    subparser.set_defaults(func=silence)

    subparser = subparsers.add_parser("status", help="status -h")
    subparser.add_argument("req_type", type=int, help="Status request type", default=0, nargs="?")
    subparser.set_defaults(func=status)

    subparser = subparsers.add_parser("tempbasal", help="tempbasal -h")
    subparser.add_argument("basalrate", type=str, help="Temporary basal rate in U/h. e.g '1.5' for 1.5U/h")
    subparser.add_argument("hours", type=str, help="Number of hours for setting the temporary basal rate. e.g '0.5' for 30 minutes")
    subparser.set_defaults(func=temp_basal)

    subparser = subparsers.add_parser("bolus", help="bolus -h")
    subparser.add_argument("units", type=str, help="amount of insulin in units to bolus")
    subparser.set_defaults(func=bolus)

    subparser = subparsers.add_parser("canceltempbasal", help="canceltempbasal -h")
    subparser.set_defaults(func=cancel_temp_basal)

    subparser = subparsers.add_parser("cancelbolus", help="cancelbolus -h")
    subparser.set_defaults(func=cancel_bolus)

    subparser = subparsers.add_parser("activate", help="activate -h")
    subparser.add_argument("utcoffset", type=int, help="utc offset for pod time in minutes")
    subparser.set_defaults(func=activate)

    subparser = subparsers.add_parser("start", help="start -h")
    subparser.add_argument("basalrate", type=str, help="Fixed basal rate in U/h. e.g '0.4' for 0.4U/h")
    subparser.set_defaults(func=start)

    subparser = subparsers.add_parser("deactivate", help="deactivate -h")
    subparser.set_defaults(func=deactivate)

    subparser = subparsers.add_parser("shutdown", help="shutdown -h")
    subparser.set_defaults(func=shutdown)

    subparser = subparsers.add_parser("restart", help="restart -h")
    subparser.set_defaults(func=restart)

    subparser = subparsers.add_parser("archive", help="archive -h")
    subparser.set_defaults(func=archive)

    args = parser.parse_args()
    pa = get_auth_params()
    args.func(args, pa)


if __name__ == '__main__':
    main()
