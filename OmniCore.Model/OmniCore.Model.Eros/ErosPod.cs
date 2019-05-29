using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Eros
{
    public class ErosPod : IPod
    {
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
        public IPodAlertStates AlertStates { get; set; }
        [Ignore]
        public IPodBasalSchedule BasalSchedule { get; set; }
        [Ignore]
        public IPodFault Fault { get; set; }
        [Ignore]
        public IPodRadioIndicators RadioIndicators { get; set; }
        [Ignore]
        public IPodStatus Status { get; set; }
        [Ignore]
        public IPodUserSettings UserSettings { get; set; }
        [Ignore]
        public ErosPodRuntimeVariables RuntimeVariables { get; set; }

        public ErosPod()
        {
            RuntimeVariables = new ErosPodRuntimeVariables();
        }
    }
}