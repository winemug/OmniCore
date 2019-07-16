using OmniCore.Mobile.Base;
using OmniCore.Model.Enums;
using OmniCore.Model.Eros;
using OmniCore.Model.Eros.Interfaces;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces;
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

namespace OmniCore.Model.Eros
{
    public partial class ErosPod : IPod
    {
        private int messageSequence;

        [PrimaryKey]
        public Guid Id { get; set; }

        public DateTimeOffset Created { get; set; }

        public uint? Lot { get; set; }
        public uint? Serial { get; set; }
        public uint RadioAddress { get; set; }
        public int MessageSequence { get => messageSequence; set => messageSequence = value % 16;
        }
        public DateTimeOffset? ActivationDate { get; set; }
        public DateTimeOffset? InsertionDate { get; set; }
        public string VersionPi { get; set; }
        public string VersionPm { get; set; }
        public string VersionUnknown { get; set; }
        public bool Archived { get; set; }
        public decimal? ReservoirUsedForPriming { get; set; }

        [Ignore]
        public IMessageExchangeResult LastTempBasalResult { get; set; }
        [Ignore]
        public IAlertStates LastAlertStates { get; set; }
        [Ignore]
        public IBasalSchedule LastBasalSchedule { get; set; }
        [Ignore]
        public IFault LastFault { get; set; }
        [Ignore]
        public IStatus LastStatus { get; set; }
        [Ignore]
        public IUserSettings LastUserSettings { get; set; }
        [Ignore]
        public ErosPodRuntimeVariables RuntimeVariables { get; set; }
        [Ignore]
        public IConversation ActiveConversation { get; set; }

        public ErosPod()
        {
            RuntimeVariables = new ErosPodRuntimeVariables();
        }
    }
}