using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Eros
{
    public class ErosPod : IPod
    {
        public uint? Id { get; set; }
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

        public IPodAlertStates AlertStates { get; set; }
        public IPodBasalSchedule BasalSchedule { get; set; }
        public IPodFault Fault { get; set; }
        public IPodRadioIndicators RadioIndicators { get; set; }
        public IPodStatus Status { get; set; }
        public IPodUserSettings UserSettings { get; set; }
        public ErosPodRuntimeVariables RuntimeVariables { get; set; }

        public ErosPod()
        {
            RuntimeVariables = new ErosPodRuntimeVariables();
        }
    }
}