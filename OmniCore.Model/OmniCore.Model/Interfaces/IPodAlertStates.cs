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


        ushort? AlertW278 { get; set; }
        ushort[] AlertStates { get; set; }
    }
}
