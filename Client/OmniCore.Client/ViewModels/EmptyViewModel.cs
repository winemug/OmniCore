using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Views.Base;
using OmniCore.Model.Interfaces.Client;

namespace OmniCore.Client.ViewModels.Base
{
    public class EmptyViewModel : BaseViewModel
    {
        public EmptyViewModel(ICoreClient client) : base(client)
        {
        }
    }
}
