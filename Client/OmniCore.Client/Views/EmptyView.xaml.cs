using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Base
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmptyView : BaseView<EmptyViewModel>
    {
        public EmptyView(EmptyViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}