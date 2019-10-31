using OmniCore.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace OmniCore.Client
{
    public class ViewViewModel<V, VM> : IViewViewModel
                                        where V : IView where VM : IViewModel
    {
        public ViewViewModel(IUnityContainer container)
        {
            container.RegisterType<IView, V>(nameof(V));
            container.RegisterType<IViewModel, VM>(nameof(VM));
        }

        public IView View { get; }

        public IViewModel ViewModel { get; }
    }
}
