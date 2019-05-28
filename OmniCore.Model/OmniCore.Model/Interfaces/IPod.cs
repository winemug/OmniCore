using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPod
    {
        uint? Id { get; set; }
        DateTime Created { get; set; }

        uint? Lot { get; set; }
        uint? Serial { get; set; }
        uint RadioAddress { get; set; }

        DateTime? ActivationDate { get; set; }
        DateTime? InsertionDate { get; set; }
        string VersionPi { get; set; }
        string VersionPm { get; set; }
        string VersionUnknown { get; set; }

        bool Archived { get; set; }

        IPodAlertStates AlertStates { get; set; }
        IPodBasalSchedule BasalSchedule { get; set; }
        IPodFault Fault { get; set; }
        IPodRadioIndicators RadioIndicators { get; set; }
        IPodStatus Status { get; set; }
        IPodUserSettings UserSettings { get; set; }
    }
}