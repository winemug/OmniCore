using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.Droid.Platform
{
    public class CoreClient : ICoreClient
    {
        public ICoreContainer<IClientResolvable> ClientContainer { get; }
        public SynchronizationContext SynchronizationContext => Application.SynchronizationContext;

        public ICoreClientConnection ClientConnection { get; }

        public IDisposable DisplayKeepAwake()
        {
            return new KeepAwakeLock();
        }

        public CoreClient(ICoreContainer<IClientResolvable> clientContainer,
            ICoreClientConnection connection)
        {
            ClientContainer = clientContainer;
            ClientConnection = connection;
        }

        public TView GetView<TView, TViewModel>(TViewModel viewModelInstance) where TView : IView<TViewModel> where TViewModel : IViewModel
        {
            var view = ClientContainer.Get<TView>();
            viewModelInstance.InitializeModel(view);
            view.SetViewModel(viewModelInstance);
            return view;
        }

        public TView GetView<TView, TViewModel>()
            where TView : IView<TViewModel>
            where TViewModel : IViewModel
        {
            return GetView<TView, TViewModel>(ClientContainer.Get<TViewModel>());
        }

        public TView GetView<TView, TViewModel, TParameter>(TViewModel viewModelInstance, TParameter parameter) where TView : IView<TViewModel> where TViewModel : IViewModel<TParameter> where TParameter : class
        {
            var view = ClientContainer.Get<TView>();
            var viewModelCast = (IViewModel<TParameter>) viewModelInstance;
            viewModelCast.InitializeModel(view, parameter);
            view.SetViewModel(viewModelInstance);
            return view;
        }

        public TView GetView<TView,TViewModel,TParameter>(TParameter parameter)
            where TView : IView<TViewModel>
            where TViewModel : IViewModel<TParameter>
            where TParameter : class
        {
            var viewModelInstance = ClientContainer.Get<TViewModel>();
            return GetView<TView, TViewModel, TParameter>(viewModelInstance, parameter);
        }
    }
}