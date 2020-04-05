using System;
using System.Collections.Generic;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Client.Platform
{
    public class ViewPresenter : IViewPresenter
    {
        private readonly ICoreContainer<IClientResolvable> ClientContainer;

        private readonly Dictionary<Type, Func<bool, object, IView>> ViewDictionary;

        public ViewPresenter(ICoreContainer<IClientResolvable> clientContainer)
        {
            ClientContainer = clientContainer;
            ViewDictionary = new Dictionary<Type, Func<bool, object, IView>>();
        }

        public IViewPresenter WithViewViewModel<TView, TViewModel>()
            where TView : IView
            where TViewModel : IViewModel
        {
            ClientContainer.One<TView>();
            ClientContainer.One<TViewModel>();

            ViewDictionary.Add(typeof(TView), (viaShell, parameter) =>
            {
                var view = ClientContainer.Get<TView>();
                var viewModel = ClientContainer.Get<TViewModel>();
                viewModel.SetParameters(view, viaShell, parameter);
                return view;
            });
            return this;
        }

        public IViewPresenter WithView<TView>() where TView : IView
        {
            ClientContainer.One<TView>();

            ViewDictionary.Add(typeof(TView), (viaShell, parameter) => { return ClientContainer.Get<TView>(); });
            return this;
        }

        public T GetView<T>(bool viaShell, object parameter = null)
            where T : IView
        {
            return (T) ViewDictionary[typeof(T)](viaShell, parameter);
        }
    }
}