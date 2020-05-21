using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
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

            var t1 = System.Console.In.ReadLineAsync();

            var cf = (ConsoleFunctions) await container.Get<IPlatformFunctions>();
            var t2 = cf.ExitEvent.WaitAsync();

            await Task.WhenAny(t1, t2);

            await api.StopServices(CancellationToken.None);
            
        }
    }
}