using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Client.ViewModels.Home
{
    public class ProgressPopupViewModel : BaseViewModel
    {
        public ITaskProgress Progress => (ITaskProgress) Parameter;

        public ProgressPopupViewModel(ICoreClient client) : base(client)
        {
        }

        protected override Task OnPageAppearing()
        {
            return base.OnPageAppearing();
        }
    }
}
