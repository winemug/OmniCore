using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmniCore.Model.Protocol;
using OmniCore.Model.Protocol.Base;
using OmniCore.Model.Protocol.Commands;
using OmniCore.Model.Protocol.Responses;

namespace OmniCore.Model
{
    [Serializable]
    public class Pdm
    {
        public Pod Pod { get; }
        public BasalSchedule BasalSchedule { get; }

        [JsonIgnore]
        public IMessageRadio Radio { get; private set; }

        [JsonIgnore]
        public string SavePath { get; set; }

        internal Pdm()
        {
        }

        public Pdm(IMessageRadio radio, Pod activePod, BasalSchedule basalSchedule)
        {
            this.Radio = radio;
            this.Pod = activePod;
            this.BasalSchedule = basalSchedule;
        }

        public static Pdm Load(IMessageRadio radio, string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (!File.Exists(path))
                return null;

            Pdm pdm = null;
            var js = new JsonSerializer();
            using (var sr = new StreamReader(path))
            {
                using (var jr = new JsonTextReader(sr))
                {
                    pdm = js.Deserialize<Pdm>(jr);
                }
            }

            if (pdm != null)
            {
                pdm.SavePath = path;
                pdm.Radio = radio;
            }

            return pdm;
        }

        public bool Save(string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = this.SavePath;
            else
                this.SavePath = path;

            if (string.IsNullOrEmpty(path))
                return false;

            var js = new JsonSerializer();
            using (var sw = new StreamWriter(path, false, Encoding.ASCII))
            {
                using (var jw = new JsonTextWriter(sw))
                {
                    js.Serialize(jw, this);
                }
            }
            return true;
        }

        public async static Task<Pdm> InitializePod(IMessageRadio radio, BasalSchedule basalSchedule)
        {
            //TODO:
            //var pod = new Pod(0, 0, 0);
            var pod = new Pod();
            
            return new Pdm(radio, pod, basalSchedule);
        }

        public async Task DeactivatePod()
        {
            //TODO:
        }

        public async Task UpdateStatus()
        {
            //TODO:
            using (var c = Conversation.Start(this.Radio, this.Pod))
            {
                var response = await c.SendRequest(new StatusRequest());
                if (response is StatusResponse)
                {
                    var sr = (StatusResponse)response;
                    this.Pod.ActiveMinutes = sr.ActiveMinutes;
                    this.Pod.Faulted = sr.FaultEvent;
                    this.Pod.Alarms = sr.Alarms;
                    this.Pod.BasalDelivery = sr.BasalState;
                    this.Pod.BolusDelivery = sr.BolusState;
                    this.Pod.DeliveredPulses = sr.DeliveredPulses;
                    this.Pod.NotDeliveredPulses = sr.NotDeliveredPulses;
                    this.Pod.Progress = sr.Progress;
                    this.Pod.Reservoir = sr.Reservoir;
                    this.Pod.MessageSequence = sr.MessageSequence;
                    this.Pod.LastUpdated = DateTime.UtcNow;
                }
                c.End();
            }
        }

        public async Task Bolus(decimal bolus)
        {
            //TODO:
        }
        public async Task CancelBolus()
        {
            //TODO:
        }

        public async Task ExtendedBolus(decimal directBolus, decimal extendedBolus, TimeSpan extendedBolusDuration)
        {
            //TODO:
        }

        public async Task CancelExtendedBolus()
        {
            //TODO:
        }

        public async Task SuspendBasal()
        {
            //TODO:
        }

        public async Task ResumeBasal()
        {
            //TODO:
        }

        public async Task UpdateBasalSchedule(BasalSchedule bs)
        {
            //TODO:
        }

        public async Task StartTempBasal()
        {
            //TODO:
        }

        public async Task CancelTempBasal()
        {
            //TODO:
        }

        public async Task AcknowledgeAlert()
        {
            //TODO:
        }

        public async Task ClearAlert()
        {
            //TODO:
        }
    }
}
