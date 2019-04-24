from .definitions import *
import simplejson as json
import time
import sqlite3

class Pod:
    def __init__(self):
        self.id_lot = None
        self.id_t = None
        self.id_version_pm = None
        self.id_version_pi = None
        self.id_version_unknown_byte = None
        self.id_version_unknown_7_bytes = None

        self.radio_address = None
        self.radio_packet_sequence = 0
        self.radio_message_sequence = 0
        self.radio_low_gain = None
        self.radio_rssi = None

        self.nonce_last = None
        self.nonce_seed = 0
        self.nonce_syncword = None

        self.state_last_updated = None
        self.state_progress = PodProgress.InitialState
        self.state_basal = BasalState.NotRunning
        self.state_bolus = BolusState.NotRunning
        self.state_alert = 0
        self.state_alerts = None
        self.state_active_minutes = 0
        self.state_faulted = False

        self.var_maximum_bolus = None
        self.var_maximum_temp_basal_rate = None
        self.var_alert_low_reservoir = None
        self.var_alert_replace_pod = None
        self.var_basal_schedule = None
        self.var_notify_bolus_start = None
        self.var_notify_bolus_cancel = None
        self.var_notify_temp_basal_set = None
        self.var_notify_temp_basal_cancel = None
        self.var_notify_basal_schedule_change = None

        self.fault_event = None
        self.fault_event_rel_time = None
        self.fault_table_access = None
        self.fault_insulin_state_table_corruption = None
        self.fault_internal_variables = None
        self.fault_immediate_bolus_in_progress = None
        self.fault_progress_before = None
        self.fault_progress_before_2 = None
        self.fault_information_type2_last_word = None

        self.insulin_reservoir = 0
        self.insulin_delivered = 0
        self.insulin_canceled = 0

        self.var_utc_offset = None
        self.var_activation_date = None
        self.var_insertion_date = None

        self.path = None
        self.path_db = None

        self.last_command = None
        self.last_command_db_id = None
        self.last_enacted_temp_basal_start = None
        self.last_enacted_temp_basal_duration = None
        self.last_enacted_temp_basal_amount = None
        self.last_enacted_bolus_start = None
        self.last_enacted_bolus_amount = None


    def Save(self, save_as = None):
        if save_as is not None:
            self.path = save_as + POD_FILE_SUFFIX
            self.path_db = save_as + POD_DB_SUFFIX

        if self.path is None:
            self.path = POD_FILE + POD_FILE_SUFFIX
            self.path_db = POD_FILE + POD_DB_SUFFIX

        try:
            self.last_command_db_id = self.log()
        except:
            pass

        try:
            with open(self.path, "w") as stream:
                json.dump(self.__dict__, stream, indent=4, sort_keys=True)
        except:
            pass

    @staticmethod
    def Load(path, db_path=None):

        if db_path is None:
            db_path = POD_FILE + POD_DB_SUFFIX

        with open(path, "r") as stream:
            d = json.load(stream)
            p = Pod()
            p.path = path
            p.path_db = db_path

            p.id_lot = d.get("id_lot", None)
            p.id_t = d.get("id_t", None)
            p.id_version_pm = d.get("id_version_pm", None)
            p.id_version_pi = d.get("id_version_pi", None)
            p.id_version_unknown_byte = d.get("id_version_unknown_byte", None)
            p.id_version_unknown_7_bytes = d.get("id_version_unknown_7_bytes", None)

            p.radio_address = d.get("radio_address", None)
            p.radio_packet_sequence = d.get("radio_packet_sequence", None)
            p.radio_message_sequence = d.get("radio_message_sequence", None)
            p.radio_low_gain = d.get("radio_low_gain", None)
            p.radio_rssi = d.get("radio_rssi", None)

            p.state_last_updated = d.get("state_last_updated", None)
            p.state_progress = d.get("state_progress", None)
            p.state_basal = d.get("state_basal", None)
            p.state_bolus = d.get("state_bolus", None)
            p.state_alert = d.get("state_alert", None)
            p.state_active_minutes = d.get("state_active_minutes", None)
            p.state_faulted = d.get("state_faulted", None)

            p.fault_event = d.get("fault_event", None)
            p.fault_event_rel_time = d.get("fault_event_rel_time", None)
            p.fault_table_access = d.get("fault_table_access", None)
            p.fault_insulin_state_table_corruption = d.get("fault_insulin_state_table_corruption", None)
            p.fault_internal_variables = d.get("fault_internal_variables", None)
            p.fault_immediate_bolus_in_progress = d.get("fault_immediate_bolus_in_progress", None)
            p.fault_progress_before = d.get("fault_progress_before", None)
            p.fault_progress_before_2 = d.get("fault_progress_before_2", None)
            p.fault_information_type2_last_word = d.get("fault_information_type2_last_word", None)

            p.insulin_delivered = d.get("insulin_delivered", None)
            p.insulin_canceled = d.get("insulin_canceled", None)
            p.insulin_reservoir = d.get("insulin_reservoir", None)

            p.nonce_last = d.get("nonce_last", None)
            p.nonce_seed = d.get("nonce_seed", None)
            p.nonce_syncword = d.get("nonce_syncword", None)

            p.last_command = d.get("last_command", None)
            p.last_enacted_temp_basal_start = d.get("last_enacted_temp_basal_start", None)
            p.last_enacted_temp_basal_duration = d.get("last_enacted_temp_basal_duration", None)
            p.last_enacted_temp_basal_amount = d.get("last_enacted_temp_basal_amount", None)
            p.last_enacted_bolus_start = d.get("last_enacted_bolus_start", None)
            p.last_enacted_bolus_amount = d.get("last_enacted_bolus_amount", None)

            p.var_utc_offset = d.get("var_utc_offset", None)
            p.var_activation_date = d.get("var_activation_date", None)
            p.var_insertion_date = d.get("var_insertion_date", None)
            p.var_basal_schedule = d.get("var_basal_schedule", None)
            p.var_maximum_bolus = d.get("var_maximum_bolus", None)
            p.var_maximum_temp_basal_rate = d.get("var_maximum_temp_basal_rate", None)
            p.var_alert_low_reservoir = d.get("var_alert_low_reservoir", None)
            p.var_alert_replace_pod = d.get("var_alert_replace_pod", None)

            p.var_notify_bolus_start = d.get("var_notify_bolus_start", None)
            p.var_notify_bolus_cancel = d.get("var_notify_bolus_cancel", None)
            p.var_notify_temp_basal_set = d.get("var_notify_temp_basal_set", None)
            p.var_notify_temp_basal_cancel = d.get("var_notify_temp_basal_cancel", None)
            p.var_notify_basal_schedule_change = d.get("var_notify_basal_schedule_change", None)

        return p

    def is_active(self):
        return not(self.id_lot is None or self.id_t is None or self.radio_address is None) \
            and (self.state_progress == PodProgress.Running or self.state_progress == PodProgress.RunningLow) \
            and not self.state_faulted


    def __str__(self):
        return json.dumps(self.__dict__, indent=4, sort_keys=True)

    def _get_conn(self):
        return sqlite3.connect(self.path_db)

    def _ensure_db_structure(self):
        with self._get_conn() as conn:
            sql = """ CREATE TABLE IF NOT EXISTS pod_history (
                      timestamp real, 
                      pod_state integer, pod_minutes integer, pod_last_command text,
                      insulin_delivered real, insulin_canceled real, insulin_reservoir real
                      ) """

            c = conn.cursor()
            c.execute(sql)

    def log(self):
        try:
            self._ensure_db_structure()
            with self._get_conn() as conn:
                sql = """ INSERT INTO pod_history (timestamp, pod_state, pod_minutes, pod_last_command,
                          insulin_delivered, insulin_canceled, insulin_reservoir)
                          VALUES(?,?,?,?,?,?,?) """

                values = (time.time(), self.state_progress, self.state_active_minutes,
                        str(self.last_command), self.insulin_delivered, self.insulin_canceled, self.insulin_reservoir)

                c = conn.cursor()
                c.execute(sql, values)
                return c.lastrowid
        except:
            getLogger().exception("Error while writing to database")

    def get_history(self):
        try:
            self._ensure_db_structure()
            # with self._get_conn() as conn:
            #     sql = "SELECT rowid, timestamp, pod_state, pod_minutes, pod_last_command," \
            #           " insulin_delivered, insulin_canceled, insulin_reservoir FROM pod_history ORDER BY rowid"
            #
            #     with conn.cursor() as c:
            #         for row in c.fetchall():
            #             print(row[4])
        except:
            getLogger().exception("Error while writing to database")
