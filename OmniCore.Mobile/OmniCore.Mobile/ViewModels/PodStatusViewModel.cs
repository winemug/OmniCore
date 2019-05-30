using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class PodStatusViewModel : BaseViewModel
    {
        private IPod Pod;
        public PodStatusViewModel()
        {
            Pod = App.PodProvider.Current.Pod;
        }

        public string Id
        {
            get
            {
                return $"L{Pod.Lot} T{Pod.Serial} 0x{Pod.RadioAddress:X8}";
            }
        }

        public string Updated
        {
            get
            {
                if (Pod.Status != null)
                    return $"{Pod.Status.Created}";
                else
                    return "Never";
            }
        }

        public string Status
        {
            get
            {
                if (Pod.Status != null)
                    return $"{Pod.Status.Progress}";
                else
                    return "Unknown";
            }
        }
    }
}
