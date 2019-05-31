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
    public class ErosPod : IPod
    {
        private IPodAlertStates alertStates;
        private IPodBasalSchedule basalSchedule;
        private IPodFault fault;
        private IPodRadioIndicators radioIndicators;
        private IPodStatus status;
        private IPodUserSettings userSettings;
        private ErosPodRuntimeVariables runtimeVariables;

        [PrimaryKey]
        public Guid? Id { get; set; }
        public DateTime Created { get; set; }

        public uint? Lot { get; set; }
        public uint? Serial { get; set; }
        public uint RadioAddress { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? InsertionDate { get; set; }
        public string VersionPi { get; set; }
        public string VersionPm { get; set; }
        public string VersionUnknown { get; set; }
        public bool Archived { get; set; }

        [Ignore]
        public IPodAlertStates AlertStates { get => alertStates; set { SetProperty(ref alertStates, value); } }
        [Ignore]
        public IPodBasalSchedule BasalSchedule { get => basalSchedule; set { SetProperty(ref basalSchedule, value); } }
        [Ignore]
        public IPodFault Fault { get => fault; set { SetProperty(ref fault, value); } }
        [Ignore]
        public IPodRadioIndicators RadioIndicators { get => radioIndicators; set { SetProperty(ref radioIndicators, value); } }
        [Ignore]
        public IPodStatus Status { get => status; set { SetProperty(ref status, value); } }
        [Ignore]
        public IPodUserSettings UserSettings { get => userSettings; set { SetProperty(ref userSettings, value); } }
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