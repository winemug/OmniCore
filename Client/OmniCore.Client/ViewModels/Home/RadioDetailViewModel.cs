using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces;

namespace OmniCore.Client.ViewModels.Home
{
    public class RadioDetailViewModel : BaseViewModel<IRadio>
    {
        public IRadio Radio { get; set; }

        public RadioDetailViewModel(ICoreClient client) : base(client)
        {
        }

        public override Task OnInitialize(IRadio parameter)
        {
            Radio = parameter;
            return Task.CompletedTask;
        }
    }
}
