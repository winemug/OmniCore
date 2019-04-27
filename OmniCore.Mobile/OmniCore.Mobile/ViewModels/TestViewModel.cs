using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        public bool BlePresent { get; set; }
        public TestViewModel()
        {
            Title = "Testing 1, 2..";

            BlePresent = (App.Instance.BleAdapter != null);
        }
    }
}
