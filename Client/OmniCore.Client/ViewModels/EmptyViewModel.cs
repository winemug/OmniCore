using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Views.Base;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Client.ViewModels.Base
{
    public class EmptyViewModel : BaseViewModel
    {
        public EmptyViewModel(ICoreClient client) : base(client)
        {
            Title = "Nothing to see here";
        }
        public override Task OnInitialize()
        {
            return Task.CompletedTask;
        }

        public override Task OnDispose()
        {
            return Task.CompletedTask;
        }
    }
}
