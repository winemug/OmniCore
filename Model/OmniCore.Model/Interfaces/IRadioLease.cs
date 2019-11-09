using OmniCore.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IRadioLease : IDisposable
    {
        IRadioConnection Connection { get; }
    }
}
