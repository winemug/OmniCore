using nexus.protocols.ble;
using OmniCore.Model;
using OmniCore.Model.Protocol.Base;
using OmniCore.Model.Utilities;
using OmniCore.Radio.RileyLink;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OmniCore.Mobile.Views
{
    public partial class TestPage : ContentPage
    {
        private readonly IPacketRadio PacketRadio;

        public TestPage()
        {
            this.PacketRadio = new RileyLink(App.Instance.BleAdapter);
            this.BindingContext = this;
            InitializeComponent();
        }

        private async Task<bool> CheckPermission(Permission p)
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(p);
            if (status != PermissionStatus.Granted)
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(p))
                {
                    await DisplayAlert("Needed", "Gonna need that permission son", "OK");
                }

                var results = await CrossPermissions.Current.RequestPermissionsAsync(p);
                //Best practice to always check that the key exists
                if (results.ContainsKey(p))
                    status = results[p];
            }

            if (status == PermissionStatus.Granted)
            {
                return true;
            }
            else if (status != PermissionStatus.Unknown)
            {
                await DisplayAlert("Permission Denied", "Can not continue, try again.", "OK");
            }
            return false;
        }
        private async void Button_ClickedAsync(object sender, EventArgs e)
        {
            if (!await CheckPermission(Permission.LocationAlways))
                return;

            if (!await CheckPermission(Permission.Storage))
                return;

            await this.PacketRadio.Initialize();

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mytestpdm.json");
            if (File.Exists(path))
                File.Delete(path);

            var radio = new SoftwareMessageRadio(this.PacketRadio);

            var pdm = Pdm.Load(radio, path);
            if (pdm == null)
            {
                var pod = new Pod() { Lot = 44152, Tid = 1220040, Address = 0x1F0E89F2 };
                var basalSchedule = new BasalSchedule(
                    new BasalEntry[] {
                    new BasalEntry(1m, new TimeSpan(0, 0, 0), new TimeSpan(4, 0, 0)),
                    new BasalEntry(0.6m, new TimeSpan(4, 0, 0), new TimeSpan(7, 0, 0)),
                    new BasalEntry(1m, new TimeSpan(7, 0, 0), new TimeSpan(12, 0, 0)),
                    new BasalEntry(1.6m, new TimeSpan(12, 0, 0), new TimeSpan(15, 0, 0)),
                    new BasalEntry(1m, new TimeSpan(15, 0, 0), new TimeSpan(17, 0, 0)),
                    new BasalEntry(0.4m, new TimeSpan(17, 0, 0), new TimeSpan(19, 0, 0)),
                    new BasalEntry(2m, new TimeSpan(19, 0, 0), new TimeSpan(24, 0, 0))
                    });
                pdm = new Pdm(radio, pod, basalSchedule);
                pdm.Save(path);
            }

            await pdm.UpdateStatus();
        }
    }
}
