using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchange
    {
        Task InitializeExchange(IMessageProgress messageProgress, CancellationToken ct);
        Task<IMessage> GetResponse(IMessage request, IMessageProgress messageProgress, CancellationToken ct);
        PodCommandResult ParseResponse(IMessage response, IPod pod);
    }
}
