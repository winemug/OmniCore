using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using OmniCore.Model.Utilities;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Eros.Data
{
    public class ErosPod : PropertyChangedImpl, IPod
    {
        private IAlertStates alertStates;
        private IBasalSchedule basalSchedule;
        private IFault fault;
        private IStatus status;
        private IUserSettings userSettings;
        private ErosPodRuntimeVariables runtimeVariables;
        private DateTime created;

        private uint? lot;
        private uint? serial;
        private uint radioAddress;
        private DateTime? activationDate;
        private DateTime? insertionDate;
        private string versionPi;
        private string versionPm;
        private string versionUnknown;
        private bool archived;
        private decimal? reservoirUsedForPriming;

        [PrimaryKey, AutoIncrement]
        public Guid? Id { get; set; }
        public DateTime Created { get => created; set { SetProperty(ref created, value); } }

        public uint? Lot { get => lot; set { SetProperty(ref lot, value); } }
        public uint? Serial { get => serial; set { SetProperty(ref serial, value); } }
        public uint RadioAddress { get => radioAddress; set { SetProperty(ref radioAddress, value); } }
        public DateTime? ActivationDate { get => activationDate; set { SetProperty(ref activationDate, value); } }
        public DateTime? InsertionDate { get => insertionDate; set { SetProperty(ref insertionDate, value); } }
        public string VersionPi { get => versionPi; set { SetProperty(ref versionPi, value); } }
        public string VersionPm { get => versionPm; set { SetProperty(ref versionPm, value); } }
        public string VersionUnknown { get => versionUnknown; set { SetProperty(ref versionUnknown, value); } }
        public bool Archived { get => archived; set { SetProperty(ref archived, value); } }
        public decimal? ReservoirUsedForPriming { get => reservoirUsedForPriming; set { SetProperty(ref reservoirUsedForPriming, value); } }

        [Ignore]
        public IMessageExchangeResult LastTempBasalResult { get; set; }
        [Ignore]
        public IAlertStates LastAlertStates { get => alertStates; set { SetProperty(ref alertStates, value); } }
        [Ignore]
        public IBasalSchedule LastBasalSchedule { get => basalSchedule; set { SetProperty(ref basalSchedule, value); } }
        [Ignore]
        public IFault LastFault { get => fault; set { SetProperty(ref fault, value); } }
        [Ignore]
        public IStatus LastStatus { get => status;
            set { SetProperty(ref status, value); } }
        [Ignore]
        public IUserSettings LastUserSettings { get => userSettings; set { SetProperty(ref userSettings, value); } }
        [Ignore]
        public ErosPodRuntimeVariables RuntimeVariables { get => runtimeVariables; set { SetProperty(ref runtimeVariables, value); } }

        public ErosPod()
        {
            RuntimeVariables = new ErosPodRuntimeVariables();
        }
    }
}