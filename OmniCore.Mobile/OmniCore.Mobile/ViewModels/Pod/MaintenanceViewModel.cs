using OmniCore.Mobile.Base;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Pod
{
    public class MaintenanceViewModel : PageViewModel
    {
        [DependencyPath(nameof(Pod), nameof(IPod.LastStatus), nameof(IStatus.Progress))]
        public string ActivationText
        {
            get
            {
                if (Pod?.LastStatus == null)
                    return "Activate New Pod";
                else if (Pod?.LastStatus?.Progress < PodProgress.Running)
                    return "Resume Activation";
                else
                    return "Pod Active";
            }
        }

        [DependencyPath(nameof(Pod), nameof(IPod.LastStatus), nameof(IStatus.Progress))]
        public bool ActivateEnabled
        {
            get
            {
                return PodNotBusy &&
                    (Pod?.LastStatus == null || Pod?.LastStatus?.Progress < PodProgress.Running);
            }
        }

        [DependencyPath(nameof(Pod), nameof(IPod.LastStatus), nameof(IStatus.Progress))]
        [DependencyPath(nameof(PodExistsAndNotBusy))]
        public bool DeactivateEnabled
        {
            get
            {
                return PodExistsAndNotBusy && Pod?.LastStatus?.Progress >= PodProgress.PairingSuccess
                    && Pod?.LastStatus?.Progress <= PodProgress.Inactive;
            }
        }

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

        protected async override Task<BaseViewModel> BindData()
        {
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
        }
    }
}
