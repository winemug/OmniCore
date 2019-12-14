using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Platform;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Client.ViewModels.Testing
{
    public class RadioDiagnosticsViewModel : BaseViewModel
    {
        public IRadio Radio { get; set; }
        public RadioDiagnosticsViewModel()
        {

        }

        public override async Task Initialize()
        {
        }

        public override async Task Dispose()
        {
        }
    }
}
