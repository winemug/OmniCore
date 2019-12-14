using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Testing;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Testing
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RadioDiagnosticsView
    {
        public RadioDiagnosticsView(RadioDiagnosticsViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}