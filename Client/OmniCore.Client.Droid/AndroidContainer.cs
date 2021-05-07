﻿using OmniCore.Client.Droid.Platform;
using OmniCore.Client.Droid.Services;
using OmniCore.Eros;
using OmniCore.Mobile.Droid.Platform;
using OmniCore.Model;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Utilities;
using OmniCore.Radios.RileyLink;
using OmniCore.Repository;
using OmniCore.Services;
using Unity;

namespace OmniCore.Client.Droid
{
    public static class AndroidContainer
    {
        public static IContainer Instance { get; }
        static AndroidContainer()
        {
            Instance = new Container(new UnityContainer());
            Instance
                .Existing(Instance)
                .One<IPlatformFunctions, PlatformFunctions>()
                .One<ILogger, Logger>()
                .One<IPlatformConfiguration, PlatformConfiguration>()
                .WithDefaultServices()
                .WithOmnipodEros()
                .WithRileyLinkRadio()
                .WithCrossBleRadioAdapter()
                .WithEfCoreRepository()
                .WithXamarinFormsClient();
        }
    }
}