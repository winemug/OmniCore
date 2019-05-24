CREATE TABLE Pods(
Id INTEGER PRIMARY KEY,
Lot INTEGER,
Serial INTEGER,
RadioAddress INTEGER NOT NULL,
VersionPm TEXT,
VersionPi TEXT,
VersionUnknown TEXT,
	UNIQUE(Lot,Serial)
);

/*

TAKEOVER - lot, tid, address

ASSIGN ADDRESS - address

SETUP POD - lot, tid, address, year, month, day, hour, minute

CONFIGURE ALERT - index, activate, trigger_auto_off, isReservoir, alert_minutes_or_reservoir, duration, beeptype, beeppattern

SET BASAL SCHEDULE - schedule, hour, min, second

BOLUS - pulses, speed, delay

STATUS REQ - type

ACK ALERTS - mask

CANCEL - bolus, tempbasal, basal

TEMPBASAL - rate, duration

DEACTIVATE

DELIVERY FLAGS - 0, 1

*/

CREATE TABLE Commands(
Id INTEGER PRIMARY KEY,
PodId INTEGER NOT NULL,
Date INTEGER NOT NULL,
	FOREIGN KEY(PodId) REFERENCES Pods(Id)
);

CREATE TABLE Statuses(
Id INTEGER PRIMARY KEY,
PodId INTEGER NOT NULL,
CommandHistoryId INTEGER NOT NULL,
Date INTEGER NOT NULL,
ActiveMinutes INTEGER NOT NULL,
Progress INTEGER NOT NULL,
BasalState INTEGER NOT NULL,
BolusState INTEGER NOT NULL,
AlertMask INTEGER NOT NULL,
InsulinReservoir REAL NOT NULL,
InsulinDelivered REAL NOT NULL,
InsulinNotDelivered REAL NOT NULL,
	FOREIGN KEY(PodId) REFERENCES Pods(Id),
	FOREIGN KEY(CommandId) REFERENCES CommandHistory(Id),
);

CREATE TABLE AlertHistory(
Id INTEGER PRIMARY KEY,
PodId INTEGER NOT NULL,
CommandHistoryId INTEGER NOT NULL,
Date INTEGER NOT NULL,
Alert0 INTEGER NOT NULL,
Alert1 INTEGER NOT NULL,
Alert2 INTEGER NOT NULL,
Alert3 INTEGER NOT NULL,
Alert4 INTEGER NOT NULL,
Alert5 INTEGER NOT NULL,
Alert6 INTEGER NOT NULL,
Alert7 INTEGER NOT NULL,
	FOREIGN KEY(PodId) REFERENCES Pods(Id),
	FOREIGN KEY(CommandId) REFERENCES CommandHistory(Id),
);

CREATE TABLE FaultHistory(
Id INTEGER PRIMARY KEY,
PodId INTEGER NOT NULL,
CommandHistoryId INTEGER NOT NULL,
Date INTEGER NOT NULL,
);


/*
CREATE TABLE BasalSchedules(
Id INTEGER PRIMARY KEY,
Name TEXT
);

CREATE TABLE BasalScheduleRates(
Id INTEGER PRIMARY KEY,
BasalScheduleId INTEGER NOT NULL,
HalfHour INTEGER NOT NULL,
Rate REAL NOT NULL,
	FOREIGN KEY(BasalScheduleId) REFERENCES BasalSchedules(Id),
	UNIQUE(BasalScheduleId,HalfHour)
);
*/