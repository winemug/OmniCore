using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using Xamarin.Forms;

namespace OmniCore.Client.Views.Base
{
    public abstract class BaseView<TModel> : ContentPage, IView<TModel>
        where TModel : IViewModel
    {
        public TModel ViewModel { get; private set; }

        public void SetViewModel(TModel viewModel)
        {
            this.ViewModel = viewModel;
            BindingContext = ViewModel;
        }
    }
}
