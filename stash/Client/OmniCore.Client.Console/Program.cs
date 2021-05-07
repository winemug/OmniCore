using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.UI.Xaml.Hosting;
using OmniCore.Client.Console.Platform;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Utilities.Extensions;

namespace OmniCore.Client.Console
{
    //dotnet tool install --global dotnet-ef
    //dotnet tool update --global dotnet-ef
    //dotnet-ef migrations add m000 --startup-project .\Client\OmniCore.Client.Console --project .\Repository\OmniCore.Repository
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var container = ConsoleContainer.Instance;

            var api = await container.Get<IServiceApi>();
            await api.StartServices(CancellationToken.None);

            await TestMenu(api);

            await api.StopServices(CancellationToken.None);
            
        }

        private static async Task TestMenu(IServiceApi api)
        {
            var user = await api.ConfigurationService.GetDefaultUser(CancellationToken.None);
            var med = await api.ConfigurationService.GetDefaultMedication(CancellationToken.None);

            while (true)
            {
                var pod = await GetTestPod(api);
                
                await System.Console.Out.WriteLineAsync("Test Menu:\n" +
                                                        (pod == null ? "(No test pod)" : "(Test pod active)") +
                                                        "\n------------------------------------------\n" +
                                                        "1) Create Test Pod\n" +
                                                        "2) Add Radios\n" +
                                                        "3) Remove Test pod\n" +
                                                        "4) Pod Action\n" +
                                                        "\n" +
                                                        "0) Exit");

                var r = await System.Console.In.ReadLineAsync();
                if (r == "0")
                    break;
                
                switch (r)
                {
                    case "1":
                        if (pod != null)
                        {
                            await System.Console.Out.WriteLineAsync("Remove existing test pod first");
                        }
                        pod = await api.PodService.NewErosPod(user, med, CancellationToken.None);
                        await pod.AsPaired(889134427, 45680, 971521, CancellationToken.None);
                        break;
                    case "2":
                        if (pod == null)
                        {
                            await System.Console.Out.WriteLineAsync("No test pod active");
                        }

                        var radios = new List<IErosRadio>();
                        await System.Console.Out.WriteLineAsync("Searching radios, press enter to stop");
                        var sub = api.PodService.ListErosRadios()
                            .Subscribe(async radio =>
                            {
                                radios.Add(radio);
                                await System.Console.Out.WriteLineAsync($"Found radio #{radios.Count} {radio.Address}");
                            });

                        await System.Console.In.ReadLineAsync();
                        sub.Dispose();

                        await System.Console.Out.WriteLineAsync($"Enter #s separated by space to assign to the pod.");
                        var rs = await System.Console.In.ReadLineAsync();
                        var ns = rs.Split(' ');
                        var toAssign = new List<IErosRadio>();
                        foreach (var n in ns)
                        {
                            if (int.TryParse(n, out int idx))
                            {
                                if (idx > 0 && idx <= radios.Count)
                                    toAssign.Add(radios[idx-1]);
                            }
                        }
                        
                        await pod.UpdateRadioList(toAssign, CancellationToken.None);
                        await System.Console.Out.WriteLineAsync($"Updated radio list with {toAssign.Count} radio(s).");
                        break;
                    case "3":
                        if (pod == null)
                        {
                            await System.Console.Out.WriteLineAsync("No test pod active");
                            break;
                        }
                        await pod.Archive(CancellationToken.None);
                        await System.Console.Out.WriteLineAsync("Done");
                        break;
                    case "4":
                        if (pod == null)
                        {
                            await System.Console.Out.WriteLineAsync("No test pod active");
                            break;
                        }

                        await pod.DebugAction(CancellationToken.None);
                        break;
                }
            }
        }

        private static async Task<IErosPod> GetTestPod(IServiceApi api)
        {
            var activePods = await api.PodService.ActivePods(CancellationToken.None);
            return (IErosPod)activePods.FirstOrDefault();
        }
        private static async Task MainMenu(IServiceApi api)
        {
            while (true)
            {
                await System.Console.Out.WriteLineAsync("Main Menu:\n" +
                                                        "------------------------------------------\n" +
                                                        "1) Pod Menu\n" +
                                                        "\n" +
                                                        "0) Exit");

                var r = await System.Console.In.ReadLineAsync();
                if (r == "0")
                    break;
                
                switch (r)
                {
                    case "1":
                        await PodMenu(api);
                        break;
                    default:
                        break;
                }
            }
        }

        private static async Task PodMenu(IServiceApi api)
        {
            IErosPod pod = null;
            
            while (true)
            {
                await System.Console.Out.WriteLineAsync("Pod Menu:\n" +
                                                        "------------------------------------------\n" +
                                                        $"Selected Pod Id: {pod?.Entity.Id}\n" +
                                                        "1) Select Pod\n" +
                                                        "2) New Pod\n" +
                                                        "\n" +
                                                        "0) Exit to main menu");

                var r = await System.Console.In.ReadLineAsync();
                if (r == "0")
                    break;
                
                switch (r)
                {
                    case "1":
                        pod = await SelectPod(api);
                        break;
                    default:
                        break;
                }
            }
        }

        private static async Task<IErosPod> SelectPod(IServiceApi api)
        {
            IErosPod pod = null;
            return pod;
        }
        
        private static async Task<IErosPod> NewPod(IServiceApi api)
        {
            IErosPod pod = null;
            return pod;
        }

    }
}