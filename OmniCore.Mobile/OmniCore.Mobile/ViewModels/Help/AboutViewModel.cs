using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.ViewModels.Help
{
    public class AboutViewModel : BaseViewModel
    {
        public string AboutText
        {
            get
            {
                return $"OmniCore v1.0.0.0";
            }
        }
    }
}
