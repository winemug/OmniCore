using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OmniCore.Mobile.ViewModels
{
    public class PodStatusViewModel : BaseViewModel
    {
        public PodStatusViewModel()
        {
        }

        private bool updateButtonEnabled = false;
        public bool UpdateButtonEnabled
        {
            get { return updateButtonEnabled; }
            set { SetProperty(ref updateButtonEnabled, value); }
        }

        protected override void OnPodPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateButtonEnabled = (Pod != null);

            if (string.IsNullOrEmpty(e.PropertyName))
                OnPropertyChanged(string.Empty);
            else
            {
                if (e.PropertyName == nameof(IPod.Lot) || e.PropertyName == nameof(IPod.Serial)
                    || e.PropertyName == nameof(IPod.RadioAddress))
                    OnPropertyChanged(nameof(Id));

                if (e.PropertyName == nameof(IPod.Status))
                {
                    OnPropertyChanged(nameof(Updated));
                    OnPropertyChanged(nameof(Status));
                }
            }

            if (e.PropertyName == nameof(IPod.Lot) || e.PropertyName == nameof(IPod.Serial)
                || e.PropertyName == nameof(IPod.RadioAddress) || e.PropertyName == nameof(IPod.Status))
            {
                OnPropertyChanged(string.Empty);
            }
        }

        public string Id
        {
            get
            {
                if (Pod == null)
                    return $"No active pod";
                else if (!Pod.Lot.HasValue)
                    return $"Lot and Serial unknown. Radio address: 0x{Pod.RadioAddress:X8}";
                else
                    return $"Lot: {Pod.Lot} Serial: {Pod.Serial} Radio address: 0x{Pod.RadioAddress:X8}";
            }
        }

        public string Updated
        {
            get
            {
                if (Pod == null)
                    return $"No active pod";
                else if (Pod.Status != null)
                    return $"{Pod.Status.Created}";
                else
                    return "Not yet updated";
            }
        }

        public string Status
        {
            get
            {
                if (Pod == null)
                    return $"No active pod";
                else if (Pod.Status != null)
                    return $"{Pod.Status.Progress}";
                else
                    return "Status unknown";
            }
        }
    }
}
