using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class PodMaintenanceViewModel : BaseViewModel
    {
        private bool deactivateButtonEnabled = false;
        public bool DeactivateButtonEnabled
        {
            get { return deactivateButtonEnabled; }
            set { SetProperty(ref deactivateButtonEnabled, value); }
        }

        private bool activateButtonEnabled = true;
        public bool ActivateButtonEnabled
        {
            get { return activateButtonEnabled; }
            set { SetProperty(ref activateButtonEnabled, value); }
        }

        private bool startButtonEnabled = false;
        public bool StartViewEnabled
        {
            get { return startButtonEnabled; }
            set { SetProperty(ref startButtonEnabled, value); }
        }

        protected override void OnPodPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DeactivateButtonEnabled = false;

            if (Pod != null && Pod.Status != null)
            {
                if (Pod.Status.Progress >= Model.Enums.PodProgress.ReadyForInjection
                    && Pod.Status.Progress < Model.Enums.PodProgress.Running)
                    StartViewEnabled = true;

                if (Pod.Status.Progress >= Model.Enums.PodProgress.PairingSuccess)
                    DeactivateButtonEnabled = true;
            }
        }
    }
}
