﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniCore.Client.ViewModels.Base;
using OmniCore.Client.Views.Wizards.NewPod;
using OmniCore.Model.Interfaces.Client;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Facade;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Home
{
    public class ActivePodsViewModel : BaseViewModel
    {
        public List<IPod> Pods { get; set; }
        public ICommand SelectCommand { get; set; }
        public ICommand AddCommand =>
            new Command(async () =>
            {
                await Client.PushView<PodWizardMainView>();
            });

        public ActivePodsViewModel(ICoreClient client) : base(client)
        {
            WhenPageAppears().Subscribe(async _ =>
            {
                Pods = new List<IPod>();
                var api= await client.GetApi(CancellationToken.None);
                foreach (var pod in await api.PodService.ActivePods(CancellationToken.None))
                    Pods.Add(pod);
            });
        }
    }
}