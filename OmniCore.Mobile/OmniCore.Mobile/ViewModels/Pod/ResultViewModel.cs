using Newtonsoft.Json;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace OmniCore.Mobile.ViewModels.Pod
{
    public class ResultViewModel : BaseViewModel
    {
        public ResultViewModel(IMessageExchangeResult result)
        {
            MessageExchangeResult = result;
            if (result.ExchangeProgress != null)
                result.ExchangeProgress.PropertyChanged += ExchangeProgress_PropertyChanged;
        }

        private IMessageExchangeResult messageExchangeResult;
        public IMessageExchangeResult MessageExchangeResult
        {
            get => messageExchangeResult;
            set
            {
                if (messageExchangeResult != value)
                {
                    if (messageExchangeResult != null)
                        messageExchangeResult.PropertyChanged -= MessageExchangeResult_PropertyChanged;

                    messageExchangeResult = value;

                    if (messageExchangeResult != null)
                        messageExchangeResult.PropertyChanged += MessageExchangeResult_PropertyChanged;

                    OnPropertyChanged(nameof(MessageExchangeResult));
                }
            }
        }

        public RowDefinitionCollection Rows
        {
            get
            {
                if (IsLive)
                    return new RowDefinitionCollection() { new RowDefinition(), new RowDefinition(), new RowDefinition() };
                else
                    return new RowDefinitionCollection() { new RowDefinition(), new RowDefinition() };
            }
        }

        public bool IsLive
        {
            get
            {
                return MessageExchangeResult.ExchangeProgress != null && !MessageExchangeResult.ExchangeProgress.Finished;
            }
        }

        public string RequestText
        {
            get
            {
                dynamic p = null;
                if (!string.IsNullOrEmpty(MessageExchangeResult.Parameters))
                    p = JsonConvert.DeserializeObject(MessageExchangeResult.Parameters);
                switch (MessageExchangeResult.Type)
                {
                    case RequestType.AssignAddress:
                        return $"Assign Address: 0x{p.Address:X8}";
                    case RequestType.SetupPod:
                        return $"Setup Pod";
                    case RequestType.SetDeliveryFlags:
                        return $"Set Delivery Flags";
                    case RequestType.PrimeCannula:
                        return $"Prime Cannula";
                    case RequestType.InsertCannula:
                        return $"Insert Cannula";
                    case RequestType.Status:
                        return $"Update Status";
                    case RequestType.AcknowledgeAlerts:
                        return $"Acknowledge Alerts";
                    case RequestType.ConfigureAlerts:
                        return $"Configure Alerts";
                    case RequestType.SetBasalSchedule:
                        return $"Set Basal Schedule";
                    case RequestType.CancelBasal:
                        return $"Cancel Basal";
                    case RequestType.SetTempBasal:
                        return $"Temp Basal {p.BasalRate:F2}U/hr {p.Duration*60:F0}min";
                    case RequestType.Bolus:
                        return $"Bolus {p.ImmediateUnits:F2}U";
                    case RequestType.CancelBolus:
                        return $"Cancel Bolus";
                    case RequestType.CancelTempBasal:
                        return $"Cancel Temp Basal";
                    case RequestType.DeactivatePod:
                        return $"Deactivate";
                    case RequestType.StartExtendedBolus:
                    case RequestType.StopExtendedBolus:
                    default:
                        return "Unknown";
                }
            }
        }

        public Color RequestTextColor
        {
            get
            {
                switch (MessageExchangeResult.Type)
                {
                    case RequestType.AssignAddress:
                    case RequestType.SetupPod:
                    case RequestType.SetDeliveryFlags:
                    case RequestType.PrimeCannula:
                    case RequestType.InsertCannula:
                        return Color.DarkOliveGreen;
                    case RequestType.Status:
                        return Color.Black;
                    case RequestType.AcknowledgeAlerts:
                    case RequestType.ConfigureAlerts:
                        return Color.DarkGray;
                    case RequestType.SetBasalSchedule:
                    case RequestType.CancelBasal:
                    case RequestType.SetTempBasal:
                    case RequestType.Bolus:
                    case RequestType.CancelBolus:
                    case RequestType.CancelTempBasal:
                    case RequestType.StartExtendedBolus:
                    case RequestType.StopExtendedBolus:
                        return Color.DarkBlue;
                    case RequestType.DeactivatePod:
                        return Color.DarkRed;
                    default:
                        return Color.Gray;
                }
            }
        }

        public string RequestDate
        {
            get
            {
                if (MessageExchangeResult.RequestTime.HasValue)
                    return MessageExchangeResult.RequestTime.Value.ToLocalTime().ToString("hh:mm:ss");
                else
                    return "";
            }
        }

        public string ResultDate
        {
            get
            {
                if (MessageExchangeResult.ResultTime.HasValue)
                    return MessageExchangeResult.ResultTime.Value.ToLocalTime().ToString("hh:mm:ss");
                else
                    return "...";
            }
        }

        public Color ResultStatusColor
        {
            get
            {
                if (MessageExchangeResult.ResultTime.HasValue)
                {
                    if (MessageExchangeResult.Success)
                        return Color.Black;
                    else
                        return Color.Red;
                }
                else
                    return Color.DarkGreen;
            }
        }

        public string ResultStatus
        {
            get
            {
                if (MessageExchangeResult.ResultTime.HasValue)
                {
                    if (MessageExchangeResult.Success)
                        return "OK";
                    else
                        return MessageExchangeResult.Failure.ToString();
                }
                else if (MessageExchangeResult.ExchangeProgress != null)
                {
                    if (MessageExchangeResult.ExchangeProgress.Waiting)
                        return "Waiting";
                    else if (MessageExchangeResult.ExchangeProgress.Running)
                        return "Running";
                    else
                        return "Finished";
                }
                else
                    return "???";
            }
        }

        public string ResultActivity
        {
            get
            {
                if (MessageExchangeResult.ExchangeProgress != null)
                {
                    return MessageExchangeResult.ExchangeProgress.ActionText;
                }
                else
                    return string.Empty;
            }
        }

        public string RileyLinkRssi
        {
            get
            {
                if (MessageExchangeResult.Statistics?.MobileDeviceRssiAverage != null)
                    return MessageExchangeResult.Statistics.MobileDeviceRssiAverage.Value.ToString();
                else
                    return "";
            }
        }

        public string PodRssi
        {
            get
            {
                if (MessageExchangeResult.Statistics?.RadioRssiAverage != null)
                    return MessageExchangeResult.Statistics.RadioRssiAverage.Value.ToString();
                else
                    return "";
            }
        }


        private void ExchangeProgress_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaiseAllPropertiesChanged();
        }

        private void MessageExchangeResult_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaiseAllPropertiesChanged();
        }

        private void RaiseAllPropertiesChanged()
        {
            OnPropertyChanged(nameof(Rows));
            OnPropertyChanged(nameof(IsLive));
            OnPropertyChanged(nameof(ResultActivity));
            OnPropertyChanged(nameof(RequestText));
            OnPropertyChanged(nameof(RequestTextColor));
            OnPropertyChanged(nameof(RequestDate));
            OnPropertyChanged(nameof(ResultDate));
            OnPropertyChanged(nameof(ResultStatusColor));
            OnPropertyChanged(nameof(ResultStatus));
            OnPropertyChanged(nameof(RileyLinkRssi));
            OnPropertyChanged(nameof(PodRssi));
        }

        protected override void OnDisposeManagedResources()
        {
            if (MessageExchangeResult.ExchangeProgress != null)
                MessageExchangeResult.ExchangeProgress.PropertyChanged -= ExchangeProgress_PropertyChanged;
            MessageExchangeResult = null;
        }
    }
}
