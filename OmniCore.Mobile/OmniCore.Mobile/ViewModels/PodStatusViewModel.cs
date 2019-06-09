using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

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

                if (e.PropertyName == nameof(IPod.ReservoirUsedForPriming))
                {
                    OnPropertyChanged(nameof(ReservoirColor));
                    OnPropertyChanged(nameof(ReservoirDelivered));
                    OnPropertyChanged(nameof(ReservoirRemaining));
                }

                if (e.PropertyName == nameof(IPod.Status))
                {
                    OnPropertyChanged(nameof(Updated));
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(LifetimeActive));
                    OnPropertyChanged(nameof(LifetimeRemaining));
                    OnPropertyChanged(nameof(LifetimeColor));
                    OnPropertyChanged(nameof(ReservoirColor));
                    OnPropertyChanged(nameof(ReservoirDelivered));
                    OnPropertyChanged(nameof(ReservoirRemaining));
                }
            }
        }

        public string Id
        {
            get
            {
                if (Pod == null)
                    return $"No active pod";
                else if (!Pod.Lot.HasValue)
                    return $"R0x{Pod.RadioAddress:X8}";
                else
                    return $"L{Pod.Lot} T{Pod.Serial} R0x{Pod.RadioAddress:X8}";
            }
        }

        public string Updated
        {
            get
            {
                if (Pod == null)
                    return string.Empty;
                else if (Pod.Status != null)
                    return $"{Pod.Status.Created.ToLocalTime()}";
                else
                    return "Not yet updated";
            }
        }

        public string Status
        {
            get
            {
                if (Pod == null)
                    return string.Empty;
                else if (Pod.Status != null)
                    return $"{Pod.Status.Progress}";
                else
                    return "Unknown";
            }
        }

        public string LifetimeRemaining
        {
            get
            {
                if (Pod == null)
                    return string.Empty;
                else if (Pod.Status != null)
                {
                    var ts = TimeSpan.FromMinutes(4800 - Pod.Status.ActiveMinutes);
                    return $"{ts.Days}d {ts.Hours}h {ts.Minutes}m";
                }
                else
                    return "Unknown";
            }
        }

        public string LifetimeActive
        {
            get
            {
                if (Pod == null)
                    return string.Empty;
                else if (Pod.Status != null)
                {
                    var ts = TimeSpan.FromMinutes(Pod.Status.ActiveMinutes);
                    return $"{ts.Days}d {ts.Hours}h {ts.Minutes}m";
                }
                else
                    return "Unknown";
            }
        }

        public Color LifetimeColor
        {
            get
            {
                if (Pod == null)
                    return Color.Beige;
                else if (Pod.Status != null)
                {
                    var mins = Pod.Status.ActiveMinutes;
                    if (mins < 24 * 60 * 3)
                        return Color.LightGreen;
                    if (mins < 24 * 60 * 3)
                        return Color.Green;
                    if (mins < 24 * 60 * 3)
                        return Color.GreenYellow;

                    return Color.IndianRed;
                }
                else
                    return Color.Beige;
            }
        }

        public double LifetimeProgress
        {
            get
            {
                if (Pod == null)
                    return 0;
                else if (Pod.Status != null)
                {
                    if (Pod.Status.ActiveMinutes >= 4800)
                        return 0;
                    return ((4800 - Pod.Status.ActiveMinutes) / 4800.0);
                }
                else
                    return 0;
            }
        }

        public string ReservoirRemaining
        {
            get
            {
                if (Pod == null)
                    return string.Empty;
                else if (Pod.Status != null)
                {
                    if (Pod.Status.Progress < PodProgress.RunningLow)
                        return "More than 50U";

                    return $"{Pod.Status.Reservoir}U";
                }
                else
                    return "Unknown";
            }
        }

        public string ReservoirDelivered
        {
            get
            {
                if (Pod == null)
                    return string.Empty;
                else if (Pod.Status != null)
                {
                    if (Pod.ReservoirUsedForPriming.HasValue)
                        return $"{Pod.Status.DeliveredInsulin - Pod.ReservoirUsedForPriming.Value}U";
                    else
                        return $"{Pod.Status.DeliveredInsulin}U (incl. reservoir used for priming)";
                }
                else
                    return "Unknown";
            }
        }

        public Color ReservoirColor
        {
            get
            {
                if (Pod == null)
                    return Color.Beige;
                else if (Pod.Status != null)
                {
                    if (Pod.Status.Progress < PodProgress.RunningLow)
                        return Color.LightGreen;

                    if (Pod.Status.Reservoir < 10)
                        return Color.Red;
                    else
                        return Color.Yellow;
                }
                else
                    return Color.Beige;
            }
        }

        public double ReservoirProgress
        {
            get
            {
                if (Pod == null)
                    return 0;
                else if (Pod.Status != null)
                {
                    if (Pod.Status.Reservoir >= 50m)
                        return 1;
                    return (double)Pod.Status.Reservoir / 50.0;
                }
                else
                    return 0;
            }
        }
    }
}
