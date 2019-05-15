using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class TestViewModel : BaseViewModel
    {
        private bool testButtonEnabled = false;
        public bool TestButtonEnabled
        {
            get { return testButtonEnabled; }
            set { SetProperty(ref testButtonEnabled, value); }
        }

        public TestViewModel()
        {
            Title = "Testing 1, 2..";

            TestButtonEnabled = (CrossBleAdapter.Current != null);
        }
    }
}
