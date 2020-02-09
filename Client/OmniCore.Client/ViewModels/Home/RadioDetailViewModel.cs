using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.Models;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services.Facade;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class RadioDetailViewModel : BaseViewModel
    {
        public RadioModel Radio { get; set; }
        public ICommand Test1Command { get; set; }
        public RadioDetailViewModel(ICoreClient client) : base(client)
        {
            Test1Command = new Command(() => { });
        }

        protected override Task OnPageAppearing()
        {
            Radio = new RadioModel((IRadio)Parameter);
            return Task.CompletedTask;
        }
    }
}
