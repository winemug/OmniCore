using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Mobile.ViewModels
{
    public class FlyoutViewModel : BaseViewModel
    {
        public string Title { get; set; }

        public ObservableCollection<TabViewModel> Tabs { get; set; }

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected override async Task<BaseViewModel> BindData()
        {
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
            if (Tabs != null)
            {
                foreach (IDisposable tab in Tabs)
                    tab.Dispose();
            }
        }
    }
}
