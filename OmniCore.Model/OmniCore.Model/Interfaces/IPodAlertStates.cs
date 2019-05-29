using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodAlertStates
    {
        uint? Id { get; set; }
        DateTime Created { get; set; }
        Guid PodId { get; set; }


        uint AlertW278 { get; set; }
        uint[] AlertStates { get; set; }
    }
}
