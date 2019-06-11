using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Eros
{
    [Table("Eros")]
    public class ErosPod : IPod
    {
        private IPodAlertStates alertStates;
        private IPodBasalSchedule basalSchedule;
        private IPodFault fault;
        private IPodRadioIndicators radioIndicators;
        private IPodStatus status;
        private IPodUserSettings userSettings;
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

        [PrimaryKey]
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
        public IPodAlertStates LastAlertStates { get => alertStates; set { SetProperty(ref alertStates, value); } }
        [Ignore]
        public IPodBasalSchedule LastBasalSchedule { get => basalSchedule; set { SetProperty(ref basalSchedule, value); } }
        [Ignore]
        public IPodFault LastFault { get => fault; set { SetProperty(ref fault, value); } }
        [Ignore]
        public IPodRadioIndicators LastRadioIndicators { get => radioIndicators; set { SetProperty(ref radioIndicators, value); } }
        [Ignore]
        public IPodStatus LastStatus { get => status;
            set { SetProperty(ref status, value); } }
        [Ignore]
        public IPodUserSettings LastUserSettings { get => userSettings; set { SetProperty(ref userSettings, value); } }
        [Ignore]
        public ErosPodRuntimeVariables RuntimeVariables { get => runtimeVariables; set { SetProperty(ref runtimeVariables, value); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public ErosPod()
        {
            RuntimeVariables = new ErosPodRuntimeVariables();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}