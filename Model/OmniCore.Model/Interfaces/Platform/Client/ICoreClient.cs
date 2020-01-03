using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces;

namespace OmniCore.Model.Interfaces
{
    public interface ICoreClient : ICoreClientFunctions
    {
        ICoreContainer<IClientResolvable> ClientContainer { get; }
        ICoreClientConnection ClientConnection { get; }
        SynchronizationContext SynchronizationContext { get; }
        IDisposable DisplayKeepAwake();

        TView GetView<TView, TViewModel>(TViewModel viewModelInstance)
            where TView : IView<TViewModel>
            where TViewModel : IViewModel;

        TView GetView<TView, TViewModel>()
            where TView : IView<TViewModel>
            where TViewModel : IViewModel;

        TView GetView<TView, TViewModel, TParameter>(TViewModel viewModelInstance, TParameter parameter)
            where TView : IView<TViewModel>
            where TViewModel : IViewModel<TParameter>
            where TParameter : class;

        TView GetView<TView, TViewModel, TParameter>(TParameter parameter)
            where TView : IView<TViewModel>
            where TViewModel : IViewModel<TParameter>
            where TParameter : class;

    }
}