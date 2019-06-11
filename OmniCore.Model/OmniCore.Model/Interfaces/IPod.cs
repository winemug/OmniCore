using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enums;

namespace OmniCore.Model.Interfaces
{
    public interface IPod : INotifyPropertyChanged
    {
        Guid? Id { get; set; }
        DateTime Created { get; set; }

        uint? Lot { get; set; }
        uint? Serial { get; set; }
        uint RadioAddress { get; set; }

        DateTime? ActivationDate { get; set; }
        DateTime? InsertionDate { get; set; }
        string VersionPi { get; set; }
        string VersionPm { get; set; }
        string VersionUnknown { get; set; }
        decimal? ReservoirUsedForPriming { get; set; }

        bool Archived { get; set; }

        IPodAlertStates LastAlertStates { get; set; }
        IPodBasalSchedule LastBasalSchedule { get; set; }
        IPodFault LastFault { get; set; }
        IPodStatus LastStatus { get; set; }
        IPodUserSettings LastUserSettings { get; set; }
    }
}