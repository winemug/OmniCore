using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Common;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class RadioDetailViewModel : BaseViewModel<IRadio>
    {
        public RadioModel Radio { get; set; }
        public ICommand Test1Command { get; set; }
        public RadioDetailViewModel(ICoreClient client) : base(client)
        {
            Test1Command = new Command(() => { });
        }

        public override Task OnInitialize(IRadio parameter)
        {
            Radio = new RadioModel(parameter);
            return Task.CompletedTask;
        }
    }
}
