using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Entities;
using OmniCore.Model.Interfaces;

namespace OmniCore.Eros
{
    public class ErosPodRequest : ErosTask, IPodRequest
    {
        public IPodRequestEntity Entity { get; set; }
        public IPod Pod { get; set; }

        public ErosPodRequest()
        {
        }

#pragma warning disable CS0067 // The event 'ErosPodRequest.PropertyChanged' is never used
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // The event 'ErosPodRequest.PropertyChanged' is never used
    }
}