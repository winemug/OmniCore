using Newtonsoft.Json;
using Omni.Py;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Py
{
    public class OmnipyStub
    {
        private string PodPath;

        public Pdm Pdm { get; private set; }
        public RileyLink PacketRadio { get; private set; }

        public OmnipyStub()
        {
            this.PodPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pod.json");
            this.PacketRadio = new RileyLink();
            this.Pdm = new Pdm(this.PacketRadio);
        }

        public void NewPod(uint radioAddress, uint lot, uint tid)
        {
            var pod = new Pod();
            pod.radio_address = radioAddress;
            pod.id_lot = lot;
            pod.id_t = tid;

            this.Pdm.Pod = pod;
        }

        private void LoadPod()
        {
            if (string.IsNullOrEmpty(this.PodPath))
                throw new ArgumentException();

            if (!File.Exists(this.PodPath))
            {
                this.Pdm.Pod = new Pod();
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
                this.Pdm.Pod = pod;
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
                    js.Serialize(jw, this.Pdm.Pod);
                }
            }
        }
    }
}
