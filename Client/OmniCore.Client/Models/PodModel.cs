﻿using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Client.Models
{
    public class PodModel
    {
        public readonly IPod Pod;

        public PodModel(IPod pod)
        {
            Pod = pod;
        }
    }
}