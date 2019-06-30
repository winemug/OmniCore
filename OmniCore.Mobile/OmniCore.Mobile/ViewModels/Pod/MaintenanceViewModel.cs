using OmniCore.Mobile.Base;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Pod
{
    public class MaintenanceViewModel : BaseViewModel
    {
        [DependencyPath(nameof(Pod), nameof(IPod.LastStatus), nameof(IStatus.Progress))]
        public bool DeactivateButtonVisible
        {
            get
            {
                return (Pod != null &&
                    (Pod.LastStatus == null ||
                    (Pod.LastStatus.Progress >= Model.Enums.PodProgress.PairingSuccess
                    && Pod.LastStatus.Progress < Model.Enums.PodProgress.Inactive)));
            }
        }

        [DependencyPath(nameof(Pod), nameof(IPod.LastStatus), nameof(IStatus.Progress))]
        public bool ActivateNewButtonVisible
        {
            get
            {
                return (Pod == null || Pod.LastStatus == null || Pod.LastStatus.Progress < Model.Enums.PodProgress.PairingSuccess);
            }
        }

        [DependencyPath(nameof(Pod), nameof(IPod.LastStatus), nameof(IStatus.Progress))]
        public bool ResumeActivationButtonVisible
        {
            get
            {
                return (Pod?.LastStatus != null && Pod.LastStatus.Progress < Model.Enums.PodProgress.Running &&
                    Pod.LastStatus.Progress >= Model.Enums.PodProgress.PairingSuccess);
            }
        }

        public MaintenanceViewModel(Page page):base(page)
        {
        }

        protected async override Task<object> BindData()
        {
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
        }
    }
}
