using Newtonsoft.Json;
using nexus.protocols.ble;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OmniCore.Py
{
    public class OmnipyStub
    {
        private readonly IBluetoothLowEnergyAdapter Ble;
        private string PodPath;

        public Pdm Pdm { get; private set; }
        public Pod Pod { get; private set; }

        public OmnipyStub(IBluetoothLowEnergyAdapter ble)
        {
            this.Ble = ble;
            this.PodPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pod.json");
            LoadPod();
            this.Pdm = new Pdm(this.Pod, new pr_rileylink(this.Ble));
        }

        public void NewPod(uint radioAddress, int lot, int tid)
        {
            var pod = new Pod();
            pod.radio_address = radioAddress;
            pod.id_lot = lot;
            pod.id_t = tid;

            SavePod();
            this.Pod = pod;
            this.Pdm = new Pdm(this.Pod, new pr_rileylink(this.Ble));
        }

        private void LoadPod()
        {
            if (string.IsNullOrEmpty(this.PodPath))
                throw new ArgumentException();

            if (!File.Exists(this.PodPath))
            {
                this.Pod = new Pod();
                SavePod();
            }
            else
            {
                Pod pod = null;
                var js = new JsonSerializer();
                using (var sr = new StreamReader(this.PodPath))
                {
                    using (var jr = new JsonTextReader(sr))
                    {
                        pod = js.Deserialize<Pod>(jr);
                    }
                }
            }
        }

        public void SavePod()
        {
            if (string.IsNullOrEmpty(this.PodPath))
                throw new ArgumentException();

            var js = new JsonSerializer();
            using (var sw = new StreamWriter(this.PodPath, false, Encoding.UTF8))
            {
                using (var jw = new JsonTextWriter(sw))
                {
                    js.Serialize(jw, this.Pod);
                }
            }
        }
    }
}
