using Newtonsoft.Json;
using OmniCore.Mobile.Base;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
                    messageExchangeResult = value;
                    OnPropertyChanged(nameof(MessageExchangeResult));
                }
            }
        }

        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.Parameters))]
        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.Type))]
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

        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.Type))]
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

        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.RequestTime))]
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

        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.ResultTime))]
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

        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.ResultTime))]
        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.Success))]
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

        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.ResultTime))]
        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.Success))]
        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.Failure))]
        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.ExchangeProgress), nameof(IMessageExchangeProgress.Waiting))]
        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.ExchangeProgress), nameof(IMessageExchangeProgress.Running))]
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

        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.ExchangeProgress), nameof(IMessageExchangeProgress.ActionText))]
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

        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.Statistics), nameof(IMessageExchangeStatistics.MobileDeviceRssiAverage))]
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

        [DependencyPath(nameof(MessageExchangeResult), nameof(IMessageExchangeResult.Statistics), nameof(IMessageExchangeStatistics.RadioRssiAverage))]
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


        protected override void OnDisposeManagedResources()
        {
        }

        protected async override Task<BaseViewModel> BindData()
        {
            return this;
        }
    }
}
