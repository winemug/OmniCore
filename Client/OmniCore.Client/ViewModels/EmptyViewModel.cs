using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Views.Base;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Client.ViewModels.Base
{
    public class EmptyViewModel : BaseViewModel
    {
        public EmptyViewModel(ICoreClient client) : base(client)
        {
        }
    }
}
