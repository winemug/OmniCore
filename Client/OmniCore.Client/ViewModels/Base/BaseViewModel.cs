using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.ViewModels.Base
{
    public abstract class BaseViewModel : IViewModel
    {
        protected ICoreServices Services { get; private set; }
        protected ICoreClient Client { get; private set; }

        private ICoreServicesConnection ServicesConnection;
        public BaseViewModel(ICoreClient client)
        {
            Client = client;
            Services = client.ServicesConnection.CoreServices;
        }

        public async Task Initialize()
        {
            await OnInitialize();
        }

        public async Task Dispose()
        {
            await OnDispose();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public string Title { get; set; }
        public IView<IViewModel> View { get; set; }
        public abstract Task OnInitialize();
        public abstract Task OnDispose();
    }
}
