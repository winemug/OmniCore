using Newtonsoft.Json;
using OmniCore.Mobile.Base;
using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        [method: SuppressMessage("", "CS1998", Justification = "Not applicable")]
        protected async override Task<BaseViewModel> BindData()
        {
            return this;
        }

        protected override void OnDisposeManagedResources()
        {
        }

        public IMessageExchangeResult MessageExchangeResult { get; set; }

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
                if (MessageExchangeResult.Success)
                    return "OK";
                else
                    return MessageExchangeResult.Failure.ToString();
            }
        }
    }
}
