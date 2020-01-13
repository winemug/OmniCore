using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Utilities
{
    public class SubTaskProgress  : ISubTaskProgress
    {
        public double? PercentComplete { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProgressPhase Phase { get; set; }
        public string ActivityDetail { get; set; }
    }
}
