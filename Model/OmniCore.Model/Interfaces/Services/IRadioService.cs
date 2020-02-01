using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Common
{
    public interface IRadioService : ICoreService
    {
        string Description { get; }
        IObservable<IRadio> ListRadios();
        IObservable<IBlePeripheral> ScanRadios();
        Task<bool> VerifyPeripheral(IBlePeripheral peripheral);
    }
}