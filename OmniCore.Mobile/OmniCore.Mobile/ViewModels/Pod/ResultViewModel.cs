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

        public string Type
        {
            get
            {
                return MessageExchangeResult.Type.ToString();
            }
        }

        public Color RequestTypeColor
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
                    return MessageExchangeResult.RequestTime.Value.ToString("hh:mm:ss");
                else
                    return "";
            }
        }

        public string ResultDate
        {
            get
            {
                if (MessageExchangeResult.ResultTime.HasValue)
                    return MessageExchangeResult.ResultTime.Value.ToString("hh:mm:ss");
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
                else
                    return "(Running)";
            }
        }

        public string RileyLinkRssi
        {
            get
            {
                if (MessageExchangeResult.Statistics.MobileDeviceRssiAverage.HasValue)
                    return MessageExchangeResult.Statistics.MobileDeviceRssiAverage.Value.ToString();
                else
                    return "";
            }
        }

        public string PodRssi
        {
            get
            {
                if (MessageExchangeResult.Statistics.RadioRssiAverage.HasValue)
                    return MessageExchangeResult.Statistics.RadioRssiAverage.Value.ToString();
                else
                    return "";
            }
        }

        private void MessageExchangeResult_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Type));
            OnPropertyChanged(nameof(RequestTypeColor));
            OnPropertyChanged(nameof(RequestDate));
            OnPropertyChanged(nameof(ResultDate));
            OnPropertyChanged(nameof(ResultStatusColor));
            OnPropertyChanged(nameof(ResultStatus));
            OnPropertyChanged(nameof(RileyLinkRssi));
            OnPropertyChanged(nameof(PodRssi));
        }

        protected override void OnDisposeManagedResources()
        {
            MessageExchangeResult = null;
        }
    }
}
