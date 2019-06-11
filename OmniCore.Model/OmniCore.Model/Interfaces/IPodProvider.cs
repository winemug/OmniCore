using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IPodProvider
    {
        IPodManager PodManager { get; }
        void Archive();
        IPodManager New();
        IPodManager Register(uint lot, uint serial, uint radioAddress);
        event EventHandler ManagerChanged;
    }
}
