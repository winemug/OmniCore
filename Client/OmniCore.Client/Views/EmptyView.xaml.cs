using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmptyView : BaseView<EmptyViewModel>
    {
        public EmptyView(EmptyViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}