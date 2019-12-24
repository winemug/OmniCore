using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IUserInterface
    {
        IObservable<IUserInterface> WhenStarting();
        IObservable<IUserInterface> WhenHibernating();
        IObservable<IUserInterface> WhenResuming();
    }
}