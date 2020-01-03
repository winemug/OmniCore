using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface ICoreClient
    {
        ICoreContainer ClientContainer { get; }
        ICoreServicesConnection ServicesConnection { get; }
        SynchronizationContext SynchronizationContext { get; }
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