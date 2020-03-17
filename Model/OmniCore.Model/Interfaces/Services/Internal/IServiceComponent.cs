using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IServiceComponent
    {
        string ComponentName { get; }
        string ComponentDescription { get; }
        bool ComponentEnabled { get; set; }
        Task InitializeComponent(ICoreService parentService);
    }
}
