DROP TABLE IF EXISTS client;
CREATE TABLE client
(
    id TEXT NOT NULL,
    name TEXT NOT NULL
);

DROP TABLE IF EXISTS profile;
CREATE TABLE profile
(
    id TEXT PRIMARY KEY,
    name TEXT
);

DROP TABLE IF EXISTS bgc;
CREATE TABLE bgc
(
    profile_id TEXT NOT NULL,
    client_id TEXT NOT NULL,
    date INTEGER NOT NULL,
    type INTEGER,
    direction INTEGER,
    value REAL,
    synced INTEGER DEFAULT 0 NOT NULL,
    deleted INTEGER DEFAULT 0 NOT NULL
);

DROP INDEX IF EXISTS bgc_reading_date;
CREATE UNIQUE INDEX bgc_reading_date ON bgc(profile_id, client_id, date);

DROP INDEX IF EXISTS bgc_date;
CREATE INDEX bgc_date ON bgc(profile_id, date);

DROP TABLE IF EXISTS pod;
CREATE TABLE pod
(
    id TEXT NOT NULL,
    profile_id TEXT NOT NULL,
    client_id TEXT NOT NULL,
    radio_address INTEGER NOT NULL,
    units_per_ml INTEGER NOT NULL,
    medication INTEGER NOT NULL,
    valid_from INTEGER NOT NULL,
    valid_to INTEGER NOT NULL,
    synced INTEGER DEFAULT 0 NOT NULL
);
DROP INDEX IF EXISTS pod_valid_from;
CREATE INDEX pod_valid_from ON pod(profile_id, valid_from);


DROP TABLE IF EXISTS pod_message;
CREATE TABLE pod_message
(
  pod_id TEXT NOT NULL,
  record_index INTEGER,
  send_start INTEGER NOT NULL,
  send_data BLOB NOT NULL,
  receive_end INTEGER,
  receive_data BLOB,
  synced INTEGER DEFAULT 0 NOT NULL
);
DROP INDEX IF EXISTS pod_record;
CREATE INDEX pod_record ON pod_message(pod_id, record_index);

DROP TABLE IF EXISTS radio;
CREATE TABLE radio
(
    mac TEXT NOT NULL,
    name TEXT NOT NULL,
    type INTEGER NOT NULL
);

DROP TABLE IF EXISTS version;
CREATE TABLE version
(
    db_version TEXT NOT NULL
);

