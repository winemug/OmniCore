using System;

namespace OmniCore.Model.Interfaces
{
    public interface IAlertStates
    {
        long? Id { get; set; }
        Guid PodId { get; set; }
        DateTimeOffset Created { get; set; }

        uint AlertW278 { get; set; }
        uint[] AlertStates { get; set; }
    }
}
