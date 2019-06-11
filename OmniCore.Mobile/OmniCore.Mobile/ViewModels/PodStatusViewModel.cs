using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

                if (e.PropertyName == nameof(IPod.LastStatus))
                {
                    OnPropertyChanged(nameof(Updated));
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(LifetimeActive));
                    OnPropertyChanged(nameof(LifetimeRemaining));
                    OnPropertyChanged(nameof(LifetimeProgress));
                    OnPropertyChanged(nameof(LifetimeColor));
                    OnPropertyChanged(nameof(ReservoirDelivered));
                    OnPropertyChanged(nameof(ReservoirRemaining));
                    OnPropertyChanged(nameof(ReservoirProgress));
                    OnPropertyChanged(nameof(ReservoirColor));
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
                else if (Pod.LastStatus != null)
                    return $"{Pod.LastStatus.Created.ToLocalTime()}";
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
                else if (Pod.LastStatus != null)
                    return $"{Pod.LastStatus.Progress}";
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
                else if (Pod.LastStatus != null)
                {
                    var ts = TimeSpan.FromMinutes(4800 - Pod.LastStatus.ActiveMinutes);
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
                else if (Pod.LastStatus != null)
                {
                    var ts = TimeSpan.FromMinutes(Pod.LastStatus.ActiveMinutes);
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
                else if (Pod.LastStatus != null)
                {
                    var mins = Pod.LastStatus.ActiveMinutes;
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
                else if (Pod.LastStatus != null)
                {
                    if (Pod.LastStatus.ActiveMinutes >= 4800)
                        return 0;
                    return ((4800 - Pod.LastStatus.ActiveMinutes) / 4800.0);
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
                else if (Pod.LastStatus != null)
                {
                    if (Pod.LastStatus.Progress < PodProgress.RunningLow)
                        return "More than 50U";

                    return $"{Pod.LastStatus.Reservoir}U";
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
                else if (Pod.LastStatus != null)
                {
                    if (Pod.ReservoirUsedForPriming.HasValue)
                        return $"{Pod.LastStatus.DeliveredInsulin - Pod.ReservoirUsedForPriming.Value}U";
                    else
                        return $"{Pod.LastStatus.DeliveredInsulin}U (incl. reservoir used for priming)";
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
                else if (Pod.LastStatus != null)
                {
                    if (Pod.LastStatus.Progress < PodProgress.RunningLow)
                        return Color.LightGreen;

                    if (Pod.LastStatus.Reservoir < 10)
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
                else if (Pod.LastStatus != null)
                {
                    if (Pod.LastStatus.Reservoir >= 50m)
                        return 1;
                    return (double)Pod.LastStatus.Reservoir / 50.0;
                }
                else
                    return 0;
            }
        }
    }
}
