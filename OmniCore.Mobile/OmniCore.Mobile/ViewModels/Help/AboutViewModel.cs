using OmniCore.Mobile.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.ViewModels.Help
{
    public class AboutViewModel : BaseViewModel
    {
        public string Version
        {
            get
            {
                return OmniCoreServices.Application.Version;
            }
        }
    }
}
