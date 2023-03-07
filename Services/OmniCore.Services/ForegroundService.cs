using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using OmniCore.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace OmniCore.Services
{
    public class ForegroundService : IForegroundService
    {
        [Unity.Dependency]
        public IRadioService RadioService { get; set; }
        
        [Unity.Dependency]
        public IAmqpService AmqpService { get; set; }
        
        [Unity.Dependency]
        public IPodService PodService { get; set; }
        
        [Unity.Dependency]
        public IDataService DataService { get; set; }
        public void Start()
        {
            Debug.WriteLine("Core services starting");
            DataService.Start();
            RadioService.Start();
            PodService.Start();
            AmqpService.Start();
            Debug.WriteLine("Core services started");
        }

        public void Stop()
        {
            Debug.WriteLine("Core services stopping");
            AmqpService.Stop();
            PodService.Stop();
            RadioService.Stop();
            DataService.Stop();
            Debug.WriteLine("Core services stopped");
        }

    }
}