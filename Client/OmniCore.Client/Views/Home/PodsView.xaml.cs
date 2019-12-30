using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Home;
using OmniCore.Client.Views.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Home
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PodsView : BaseView<PodsViewModel>
    {
        public PodsView(PodsViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
        }
    }
}