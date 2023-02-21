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
    public class CoreService : IForegroundService
    {
        [Unity.Dependency]
        public RadioService RadioService { get; set; }
        
        [Unity.Dependency]
        public AmqpService AmqpService { get; set; }
        
        [Unity.Dependency]
        public PodService PodService { get; set; }
        
        [Unity.Dependency]
        public DataService DataService { get; set; }
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