using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Client
{
    public interface IViewPresenter : IClientResolvable
    {
        IViewPresenter WithViewViewModel<TView, TViewModel>()
            where TView : IView
            where TViewModel : IViewModel;

        IViewPresenter WithView<TView>()
            where TView : IView;

        T GetView<T>(bool viaShell, object parameter = null)
            where T : IView;
    }
}
