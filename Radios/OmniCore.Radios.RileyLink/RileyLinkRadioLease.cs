using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace OmniCore.Radios.RileyLink
{
    public class RileyLinkRadioLease : IRadioLease
    {
        public IRadioConnection Connection { get; set; }
        private SemaphoreSlim LeaseSemaphore;

        public RileyLinkRadioLease(SemaphoreSlim leaseSemaphore)
        {
            LeaseSemaphore = leaseSemaphore;
        }

        public void Dispose()
        {
            LeaseSemaphore?.Release();
        }
    }
}
