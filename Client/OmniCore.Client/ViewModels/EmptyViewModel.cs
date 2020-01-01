using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Client.Views.Base;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.ViewModels.Base
{
    public class EmptyViewModel : BaseViewModel
    {
        public EmptyViewModel(ICoreServices services) : base(services)
        {
            Title = "Nothing to see here";
        }
        public override Task Initialize()
        {
            return Task.CompletedTask;
        }

        public override Task Dispose()
        {
            return Task.CompletedTask;
        }
    }
}
