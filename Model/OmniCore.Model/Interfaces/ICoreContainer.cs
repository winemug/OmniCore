using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Platform;

namespace OmniCore.Model.Interfaces
{
    public interface ICoreContainer
    {
        ICoreContainer Many<T>();
        ICoreContainer Many<TI, TC>() where TC : TI;
        ICoreContainer One<T>();
        ICoreContainer One<TI, TC>() where TC : TI;
        ICoreContainer Existing<T>(T instance);
        T Get<T>();
        T[] GetAll<T>();
    }
}
