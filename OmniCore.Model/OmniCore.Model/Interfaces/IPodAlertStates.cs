using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodAlertStates
    {
        long? Id { get; set; }
        long ResultId { get; set; }


        uint AlertW278 { get; set; }
        uint[] AlertStates { get; set; }
    }
}
