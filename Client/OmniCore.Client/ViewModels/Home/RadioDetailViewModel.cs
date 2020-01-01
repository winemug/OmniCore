using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.ViewModels.Home
{
    public class RadioDetailViewModel : BaseViewModel
    {
        public IRadio Radio { get; set; }
        public override async Task OnInitialize()
        {
        }

        public override async Task OnDispose()
        {
        }

        public RadioDetailViewModel(ICoreClient client) : base(client)
        {
        }
    }
}
