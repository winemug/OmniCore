from .protocol import *
from .protocol_radio import PdmRadio
from .nonce import *
from .exceptions import PdmError, OmnipyError, PdmBusyError
from .definitions import *
from .packet_radio import TxPower
from decimal import *
from datetime import datetime, timedelta
from threading import RLock
import time


g_lock = RLock()


class PdmLock():
    def __init__(self, timeout=2):
        self.fd = None
        self.timeout = timeout

    def __enter__(self):
        if not g_lock.acquire(blocking=True, timeout=self.timeout):
            raise PdmBusyError()

    def __exit__(self, exc_type, exc_val, exc_tb):
        g_lock.release()


class Pdm:
    def __init__(self, pod):
        if pod is None:
            raise PdmError("Cannot instantiate pdm without pod")

        self.pod = pod
        self.nonce = None
        self.radio = None
        self.time_adjustment = 0
        self.logger = getLogger()

    def stop_radio(self):
        if self.radio is not None:
            self.radio.stop()
            self.radio = None

    def start_radio(self):
        self.get_radio(new=True)

    def get_nonce(self):
        if self.nonce is None:
            if self.pod.id_lot is None or self.pod.id_t is None:
                return None
            if self.pod.nonce_last is None or self.pod.nonce_seed is None:
                self.nonce = Nonce(self.pod.id_lot, self.pod.id_t)
            else:
                self.nonce = Nonce(self.pod.id_lot, self.pod.id_t, self.pod.nonce_last, self.pod.nonce_seed)
        return self.nonce

    def get_radio(self, new=False):
        if self.radio is not None and new:
            self.radio.stop()
            self.radio = None

        if self.radio is None:
            if self.pod.radio_message_sequence is None or self.pod.radio_packet_sequence is None:
                self.pod.radio_message_sequence = 0
                self.pod.radio_packet_sequence = 0

            self.radio = PdmRadio(self.pod.radio_address,
                                  msg_sequence=self.pod.radio_message_sequence,
                                  pkt_sequence=self.pod.radio_packet_sequence)

        return self.radio

    def send_request(self, request, with_nonce=False, double_take=False,
                        expect_critical_follow_up=False,
                        tx_power=TxPower.Normal):

        nonce_obj = self.get_nonce()
        if with_nonce:
            nonce_val = nonce_obj.getNext()
            request.set_nonce(nonce_val)
            self.pod.nonce_syncword = None

        response = self.get_radio().send_message_get_message(request, double_take=double_take,
                                                             expect_critical_follow_up=expect_critical_follow_up,
                                                             tx_power=tx_power)
        response_parse(response, self.pod)

        if with_nonce and self.pod.nonce_syncword is not None:
            self.logger.info("Nonce resync requested")
            nonce_obj.sync(self.pod.nonce_syncword, request.sequence)
            nonce_val = nonce_obj.getNext()
            request.set_nonce(nonce_val)
            self.pod.nonce_syncword = None
            self.get_radio().message_sequence = request.sequence
            response = self.get_radio().send_message_get_message(request, double_take=double_take,
                                                                 expect_critical_follow_up=expect_critical_follow_up)
            response_parse(response, self.pod)
            if self.pod.nonce_syncword is not None:
                self.get_nonce().reset()
                raise PdmError("Nonce sync failed")

    def _internal_update_status(self, update_type=0):
        self._assert_pod_address_assigned()
        self.send_request(request_status(update_type))

    def update_status(self, update_type=0):
        try:
            with PdmLock():
                self.logger.info("Updating pod status, request type %d" % update_type)
                self.pod.last_command = { "command": "STATUS", "type": update_type, "success": False }
                self._internal_update_status(update_type)
                self.pod.last_command["success"] = True
        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()

    def acknowledge_alerts(self, alert_mask):
        try:
            with PdmLock():
                self.logger.info("Acknowledging alerts with bitmask %d" % alert_mask)
                self.pod.last_command = {"command": "ACK_ALERTS", "mask": alert_mask, "success": False}
                self._assert_pod_address_assigned()
                self._internal_update_status()
                self._assert_can_acknowledge_alerts()

                if self.pod.state_alert | alert_mask != self.pod.state_alert:
                    raise PdmError("Bitmask invalid for current alert state")

                request = request_acknowledge_alerts(alert_mask)
                self.send_request(request, with_nonce=True)
                if self.pod.state_alert & alert_mask != 0:
                    raise PdmError("Failed to acknowledge one or more alerts")
                self.pod.last_command["success"] = True
        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()

    # def configure_reservoir_alarm(self, iu_reservoir_level=None):
    #     try:
    #         with PdmLock(0):
    #             if iu_reservoir_level is None:
    #                 request = request_clear_low_reservoir_alert()
    #             else:
    #                 request = request_set_low_reservoir_alert(self.pod.var_alert_low_reservoir)
    #             self.send_request(request, with_nonce=True)
    #             self.pod.var_alert_low_reservoir_set = True
    #     except OmnipyError:
    #         raise
    #     except Exception as e:
    #         raise PdmError("Unexpected error") from e
    #
    # def configure_pod_expiry_alarm(self, minutes_after_activation=None):
    #     try:
    #         with PdmLock(0):
    #             if minutes_after_activation is None:
    #                 request = request_clear_pod_expiry_alert()
    #             else:
    #                 request = request_set_pod_expiry_alert(minutes_after_activation)
    #             self.send_request(request, with_nonce=True)
    #             self.pod.var_alert_low_reservoir_set = True
    #     except OmnipyError:
    #         raise
    #     except Exception as e:
    #         raise PdmError("Unexpected error") from e
    def hf_silence_will_fall(self):
        try:
            with PdmLock():
                self._internal_update_status()
                if self.pod.state_alert > 0:
                    self.logger.info("Acknowledging alerts with bitmask %d" % self.pod.state_alert)
                    self.pod.last_command = {"command": "ACK_ALERTS", "mask": self.pod.state_alert, "success": False}
                    request = request_acknowledge_alerts(self.pod.state_alert)
                    self.send_request(request, with_nonce=True)
                    self.pod.last_command = {"command": "ACK_ALERTS", "mask": self.pod.state_alert, "success": False}

                self._internal_update_status(1)

                active_alerts = []
                if self.pod.state_alerts is not None:
                    for ai in range(0,8):
                        if self.pod.state_alerts[ai] > 0:
                            active_alerts.append(ai)

                if len(active_alerts) == 0:
                    self.logger.info("No alerts active")
                else:
                    self.logger.info("Clearing alerts: %s" % str(active_alerts))
                    acs = []
                    for i in active_alerts:
                        ac = AlertConfiguration()
                        ac.activate = False
                        ac.alert_after_minutes = 0
                        ac.alert_duration = 0
                        ac.alert_index = i
                        acs.append(ac)
                    request = request_acknowledge_alerts(self.pod.state_alert)
                    self.send_request(request, with_nonce=True)
                    self.pod.last_command["success"] = True
        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()


    def is_busy(self):
        try:
            with PdmLock(0):
                return self._is_bolus_running(no_live_check=True)
        except PdmBusyError:
            return True
        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e

    def bolus(self, bolus_amount):
        try:
            with PdmLock():
                self.pod.last_command = {"command": "BOLUS", "units": bolus_amount, "success": False}

                self._assert_pod_address_assigned()
                self._internal_update_status()
                self._assert_can_generate_nonce()
                self._assert_immediate_bolus_not_active()
                self._assert_not_faulted()
                self._assert_status_running()

                if self.pod.var_maximum_bolus is not None and bolus_amount > self.pod.var_maximum_bolus:
                    raise PdmError("Bolus exceeds defined maximum bolus of %.2fU" % self.pod.var_maximum_bolus)

                if bolus_amount < DECIMAL_0_05:
                    raise PdmError("Cannot do a bolus less than 0.05U")

                if self._is_bolus_running():
                    raise PdmError("A previous bolus is already running")

                if bolus_amount > self.pod.insulin_reservoir:
                    raise PdmError("Cannot bolus %.2f units, insulin_reservoir capacity is at: %.2f")

                self.logger.debug("Bolusing %0.2f" % float(bolus_amount))
                request = request_bolus(bolus_amount)
                self.send_request(request, with_nonce=True)

                if self.pod.state_bolus != BolusState.Immediate:
                    raise PdmError("Pod did not confirm bolus")

                self.pod.last_enacted_bolus_start = self.get_time()
                self.pod.last_enacted_bolus_amount = float(bolus_amount)
                self.pod.last_command["success"] = True
        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()


    def cancel_bolus(self):
        try:
            with PdmLock():
                self.logger.debug("Canceling bolus")
                self.pod.last_command = {"command": "BOLUS_CANCEL", "canceled": 0, "success": False}
                self._assert_pod_address_assigned()
                self._assert_can_generate_nonce()
                self._assert_not_faulted()
                self._assert_status_running()

                if self._is_bolus_running():
                    request = request_cancel_bolus()
                    self.send_request(request, with_nonce=True)
                    if self.pod.state_bolus == BolusState.Immediate:
                        raise PdmError("Failed to cancel bolus")
                    else:
                        self.pod.last_enacted_bolus_amount = float(-1)
                        self.pod.last_enacted_bolus_start = self.get_time()
                        self.pod.last_command["success"] = True
                        self.pod.last_command["canceled"] = self.pod.insulin_canceled
                else:
                    raise PdmError("Bolus is not running")

        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()

    def cancel_temp_basal(self):
        try:
            with PdmLock():
                self.logger.debug("Canceling temp basal")
                self.pod.last_command = {"command": "TEMPBASAL_CANCEL", "success": False}
                self._assert_pod_address_assigned()
                self._internal_update_status()
                self._assert_can_generate_nonce()
                self._assert_immediate_bolus_not_active()
                self._assert_not_faulted()
                self._assert_status_running()

                if self._is_temp_basal_active():
                    request = request_cancel_temp_basal()
                    self.send_request(request, with_nonce=True)
                    if self.pod.state_basal == BasalState.TempBasal:
                        raise PdmError("Failed to cancel temp basal")
                    else:
                        self.pod.last_enacted_temp_basal_duration = float(-1)
                        self.pod.last_enacted_temp_basal_start = self.get_time()
                        self.pod.last_enacted_temp_basal_amount = float(-1)
                        self.pod.last_command["success"] = True
                else:
                    self.logger.warning("Cancel temp basal received, while temp basal was not active. Ignoring.")

        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()

    def set_temp_basal(self, basalRate, hours, confidenceReminder=False):
        try:
            with PdmLock():
                self.logger.debug("Setting temp basal %02.2fU/h for %02.1fh"% (float(basalRate), float(hours)))
                self.pod.last_command = {"command": "TEMPBASAL",
                                         "duration_hours": hours,
                                         "hourly_rate": basalRate,
                                         "success": False}
                self._assert_pod_address_assigned()
                self._internal_update_status()
                self._assert_can_generate_nonce()
                self._assert_immediate_bolus_not_active()
                self._assert_not_faulted()
                self._assert_status_running()

                if hours > 12 or hours < 0.5:
                    raise PdmError("Requested duration is not valid")

                if self.pod.var_maximum_temp_basal_rate is not None and \
                        basalRate > Decimal(self.pod.var_maximum_temp_basal_rate):
                    raise PdmError("Requested rate exceeds maximum temp basal setting")
                if basalRate > Decimal(30):
                    raise PdmError("Requested rate exceeds maximum temp basal capability")

                if self._is_temp_basal_active():
                    self.logger.debug("Canceling active temp basal before setting a new temp basal")
                    request = request_cancel_temp_basal()
                    self.send_request(request, with_nonce=True)
                    if self.pod.state_basal == BasalState.TempBasal:
                        raise PdmError("Failed to cancel running temp basal")
                request = request_temp_basal(basalRate, hours)
                self.send_request(request, with_nonce=True)

                if self.pod.state_basal != BasalState.TempBasal:
                    raise PdmError("Failed to set temp basal")
                else:
                    self.pod.last_enacted_temp_basal_duration = float(hours)
                    self.pod.last_enacted_temp_basal_start = self.get_time()
                    self.pod.last_enacted_temp_basal_amount = float(basalRate)
                    self.pod.last_command["success"] = True

        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()

    def set_basal_schedule(self, schedule):
        try:
            with PdmLock():
                self.logger.debug("Setting basal schedule: %s"% schedule)
                self.pod.last_command = {"command": "BASALSCHEDULE",
                                         "hourly_rates": schedule,
                                         "success": False}
                self._assert_pod_address_assigned()
                self._internal_update_status()
                self._assert_can_generate_nonce()
                self._assert_immediate_bolus_not_active()
                self._assert_not_faulted()
                self._assert_status_running()

                if self._is_temp_basal_active():
                    raise PdmError("Cannot change basal schedule while a temp. basal is active")

                self._assert_basal_schedule_is_valid(schedule)

                pod_date = datetime.utcnow() + timedelta(minutes=self.pod.var_utc_offset) \
                           + timedelta(seconds=self.time_adjustment)

                hours = pod_date.hour
                minutes = pod_date.minute
                seconds = pod_date.second

                request = request_set_basal_schedule(schedule, hour=hours, minute=minutes, second=seconds)
                self.send_request(request, with_nonce=True, double_take=True)

                if self.pod.state_basal != BasalState.Program:
                    raise PdmError("Failed to set basal schedule")
                else:
                    self.pod.var_basal_schedule = schedule
                    self.pod.last_command["success"] = True

        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()

    def deactivate_pod(self):
        try:
            with PdmLock():
                self.logger.debug("Deactivating pod")
                self.pod.last_command = {"command": "DEACTIVATE", "success": False}
                self._internal_update_status()
                self._assert_can_deactivate()

                request = request_deactivate()
                self.send_request(request, with_nonce=True)
                if self.pod.state_progress != PodProgress.Inactive:
                    raise PdmError("Failed to deactivate")
                else:
                    self.pod.last_command["success"] = True
        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()

    def activate_pod(self, candidate_address, utc_offset):
        try:
            with PdmLock():
                self.logger.debug("Activating pod")
                self.pod.last_command = {"command": "ACTIVATE",
                                         "address": candidate_address,
                                         "utc_offset": utc_offset,
                                         "success": False}

                if self.pod.state_progress > PodProgress.ReadyForInjection:
                    raise PdmError("Pod is already activated")

                self.pod.var_utc_offset = utc_offset
                radio = None

                if self.pod.state_progress is None or \
                                self.pod.state_progress < PodProgress.TankFillCompleted:

                    self.pod.radio_address = 0xffffffff

                    radio = self.get_radio(new=True)
                    radio.radio_address = 0xffffffff

                    request = request_assign_address(candidate_address)
                    response = self.get_radio().send_message_get_message(request, message_address=0xffffffff,
                                                                         ack_address_override=candidate_address,
                                                                         tx_power=TxPower.Low)
                    response_parse(response, self.pod)

                    self._assert_pod_can_activate()

                if self.pod.state_progress == PodProgress.TankFillCompleted:

                    self.pod.var_activation_date = self.get_time()
                    pod_date = datetime.utcfromtimestamp(self.pod.var_activation_date) \
                               + timedelta(minutes=self.pod.var_utc_offset)

                    year = pod_date.year
                    month = pod_date.month
                    day = pod_date.day
                    hour = pod_date.hour
                    minute = pod_date.minute

                    if radio is None:
                        radio = self.get_radio(new=True)
                        radio.radio_address = 0xffffffff

                    radio.message_sequence = 1

                    request = request_setup_pod(self.pod.id_lot, self.pod.id_t, candidate_address,
                                                year, month, day, hour, minute)
                    response = self.get_radio().send_message_get_message(request, message_address=0xffffffff,
                                                                         ack_address_override=candidate_address,
                                                                         tx_power=TxPower.Low)
                    response_parse(response, self.pod)
                    self._assert_pod_paired()

                if self.pod.state_progress == PodProgress.PairingSuccess:
                    if radio is not None:
                        self.pod.radio_packet_sequence = radio.packet_sequence

                    radio = self.get_radio(new=True)
                    radio.radio_address = self.pod.radio_address
                    radio.message_sequence = 2

                    self.pod.nonce_seed = 0
                    self.pod.nonce_last = None

                    # if self.pod.var_alert_low_reservoir is not None:
                    #     if not self.pod.var_alert_low_reservoir_set:
                    #         request = request_set_low_reservoir_alert(self.pod.var_alert_low_reservoir)
                    #         self.send_request(request, with_nonce=True, tx_power=TxPower.Low)
                    #         self.pod.var_alert_low_reservoir_set = True
                    #
                    # if not self.pod.var_alert_before_prime_set:
                    #     request = request_set_generic_alert(5, 55)
                    #     self.send_request(request, with_nonce=True, tx_power=TxPower.Low)
                    #     self.pod.var_alert_before_prime_set = True

                    # request = request_delivery_flags(0, 0)
                    # self.send_request(request, with_nonce=True)

                    request = request_prime_cannula()
                    self.send_request(request, with_nonce=True, tx_power=TxPower.Low)

                    time.sleep(55)

                self._internal_update_status()
                while self.pod.state_progress != PodProgress.ReadyForInjection:
                    time.sleep(5)
                    self._internal_update_status()

                # if self.pod.state_progress == PodProgress.ReadyForInjection:
                #     if self.pod.var_alert_replace_pod is not None:
                #         if not self.pod.var_alert_replace_pod_set:
                #             request = request_set_pod_expiry_alert(self.pod.var_alert_replace_pod - self.pod.state_active_minutes)
                #             self.send_request(request, with_nonce=True, tx_power=TxPower.Low)
                #             self.pod.var_alert_replace_pod_set = True

                self.pod.last_command["success"] = True

        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()

    def inject_and_start(self, basal_schedule):
        try:
            with PdmLock():

                self.logger.debug("Starting pod")
                self.pod.last_command = {"command": "START",
                                         "hourly_rates": basal_schedule,
                                         "success": False}

                if self.pod.state_progress >= PodProgress.Running:
                    raise PdmError("Pod has passed the injection stage")

                if self.pod.state_progress < PodProgress.ReadyForInjection:
                    raise PdmError("Pod is not ready for injection")

                if self.pod.state_progress == PodProgress.ReadyForInjection:
                    self._assert_basal_schedule_is_valid(basal_schedule)

                    pod_date = datetime.utcnow() + timedelta(minutes=self.pod.var_utc_offset) \
                                + timedelta(seconds=self.time_adjustment)

                    hour = pod_date.hour
                    minute = pod_date.minute
                    second = pod_date.second

                    request = request_set_basal_schedule(basal_schedule, hour, minute, second)
                    self.send_request(request, with_nonce=True, double_take=True, expect_critical_follow_up=True)

                    if self.pod.state_progress != PodProgress.BasalScheduleSet:
                        raise PdmError("Pod did not acknowledge basal schedule")

                if self.pod.state_progress == PodProgress.BasalScheduleSet:
                    # if not self.pod.var_alert_after_prime_set:
                    #     request = request_set_initial_alerts(self.pod.var_activation_date)
                    #     self.send_request(request, with_nonce=True, expect_critical_follow_up=True)
                    #     self.pod.var_alert_after_prime_set = True

                    request = request_insert_cannula()
                    self.send_request(request, with_nonce=True)

                    if self.pod.state_progress != PodProgress.Inserting:
                        raise PdmError("Pod did not acknowledge cannula insertion start")

                if self.pod.state_progress == PodProgress.Inserting:
                    time.sleep(13)
                    self._internal_update_status()
                    if self.pod.state_progress != PodProgress.Running:
                        raise PdmError("Pod did not get to running state")
                    self.pod.var_insertion_date = self.get_time()
                    self.pod.last_command["success"] = True

        except OmnipyError:
            raise
        except Exception as e:
            raise PdmError("Unexpected error") from e
        finally:
            self._savePod()

    def _savePod(self):
        try:
            radio = self.get_radio()
            if radio is not None:
                self.pod.radio_message_sequence = radio.message_sequence
                self.pod.radio_packet_sequence = radio.packet_sequence

            nonce = self.get_nonce()
            if nonce is not None:
                self.pod.nonce_last = nonce.lastNonce
                self.pod.nonce_seed = nonce.seed

            return self.pod.Save()
        except Exception as e:
            raise PdmError("Pod status was not saved") from e

    def _is_bolus_running(self, no_live_check=False):
        if self.pod.state_last_updated is not None and self.pod.state_bolus != BolusState.Immediate:
            return False

        if self.pod.last_enacted_bolus_amount is not None \
                and self.pod.last_enacted_bolus_start is not None:

            if self.pod.last_enacted_bolus_amount < 0:
                return False

            now = self.get_time()
            bolus_end_earliest = (self.pod.last_enacted_bolus_amount * 39) + 1 + self.pod.last_enacted_bolus_start
            bolus_end_latest = (self.pod.last_enacted_bolus_amount * 41) + 3 + self.pod.last_enacted_bolus_start
            if now > bolus_end_latest:
                return False
            elif now < bolus_end_earliest:
                return True

        if no_live_check:
            return True

        self._internal_update_status()
        return self.pod.state_bolus == BolusState.Immediate

    def _is_basal_schedule_active(self):
        if self.pod.state_last_updated is not None and self.pod.state_basal == BasalState.NotRunning:
            return False

        self._internal_update_status()
        return self.pod.state_basal == BasalState.Program

    def _is_temp_basal_active(self):
        if self.pod.state_last_updated is not None and self.pod.state_basal != BasalState.TempBasal:
            return False

        if self.pod.last_enacted_temp_basal_start is not None \
                and self.pod.last_enacted_temp_basal_duration is not None:
            if self.pod.last_enacted_temp_basal_amount < 0:
                return False
            now = self.get_time()
            temp_basal_end_earliest = self.pod.last_enacted_temp_basal_start + \
                                      (self.pod.last_enacted_temp_basal_duration * 3600) - 60
            temp_basal_end_latest = self.pod.last_enacted_temp_basal_start + \
                                      (self.pod.last_enacted_temp_basal_duration * 3660) + 60
            if now > temp_basal_end_latest:
                return False
            elif now < temp_basal_end_earliest:
                return True

        self._internal_update_status()
        return self.pod.state_basal == BasalState.TempBasal

    def _assert_pod_activate_can_start(self):
        self._assert_pod_address_not_assigned()

    def _assert_basal_schedule_is_valid(self, schedule):
        if schedule is None:
            raise PdmError("No basal schedule defined")

        if len(schedule) != 48:
            raise PdmError("A full schedule of 48 half hours is needed")

        min_rate = Decimal("0.05")
        max_rate = Decimal("30")

        for entry in schedule:
            if entry < min_rate:
                raise PdmError("A basal rate schedule entry cannot be less than 0.05U/h")
            if entry > max_rate:
                raise PdmError("A basal rate schedule entry cannot be more than 30U/h")

    def _assert_pod_address_not_assigned(self):
        if self.pod is None:
            raise PdmError("No pod instance created")

        if self.pod.radio_address is not None and self.pod.radio_address != 0xffffffff:
            raise PdmError("Radio radio_address already set")

    def _assert_pod_address_assigned(self):
        if self.pod.radio_address is None:
            raise PdmError("Radio address not set")

    def _assert_pod_can_activate(self):
        if self.pod is None:
            raise PdmError("No pod instance created")

        if self.pod.id_lot is None:
            raise PdmError("Lot number unknown")

        if self.pod.id_t is None:
            raise PdmError("Serial number unknown")

        if self.pod.state_progress != PodProgress.TankFillCompleted:
            raise PdmError("Pod is not at the expected state of Tank Fill Completed")

    def _assert_pod_paired(self):
        if self.pod.radio_address is None or self.pod.radio_address == 0 \
                or self.pod.radio_address == 0xffffffff:
            raise PdmError("Radio radio_address not accepted")

        if self.pod.state_progress != PodProgress.PairingSuccess:
            raise PdmError("Progress does not indicate pairing success")

    def _assert_can_deactivate(self):
        self._assert_pod_address_assigned()
        self._assert_can_generate_nonce()
        if self.pod.state_progress < PodProgress.PairingSuccess:
            raise PdmError("Pod is not paired")
        if self.pod.state_progress > PodProgress.AlertExpiredShuttingDown:
            raise PdmError("Pod already deactivated")

    def _assert_can_acknowledge_alerts(self):
        self._assert_pod_address_assigned()
        if self.pod.state_progress < PodProgress.PairingSuccess:
            raise PdmError("Pod not paired completely yet.")

        if self.pod.state_progress == PodProgress.ErrorShuttingDown:
            raise PdmError("Pod is shutting down, cannot acknowledge alerts.")

        if self.pod.state_progress == PodProgress.AlertExpiredShuttingDown:
            raise PdmError("Acknowledgement period expired, pod is shutting down")

        if self.pod.state_progress > PodProgress.AlertExpiredShuttingDown:
            raise PdmError("Pod is not active")

    def _assert_can_generate_nonce(self):
        if self.pod.id_lot is None:
            raise PdmError("Lot number is not defined")

        if self.pod.id_t is None:
            raise PdmError("Pod serial number is not defined")

    def _assert_status_running(self):
        if self.pod.state_progress < PodProgress.Running:
            raise PdmError("Pod is not yet running")

        if self.pod.state_progress > PodProgress.RunningLow:
            raise PdmError("Pod has stopped")

    def _assert_not_faulted(self):
        if self.pod.state_faulted:
            raise PdmError("Pod is state_faulted")

    def _assert_no_active_alerts(self):
        if self.pod.state_alert != 0:
            raise PdmError("Pod has active alerts")

    def _assert_immediate_bolus_not_active(self):
        if self._is_bolus_running():
            raise PdmError("Pod is busy delivering a bolus")

    def set_time_adjustment(self, adjustment):
        self.time_adjustment = adjustment

    def get_time(self):
        return time.time() + self.time_adjustment