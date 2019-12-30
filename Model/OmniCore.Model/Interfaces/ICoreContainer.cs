using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Model.Interfaces
{
    public interface ICoreContainer
    {
        ICoreContainer Many<T>();
        ICoreContainer Many<TI, TC>() where TC : TI;
        ICoreContainer One<T>();
        ICoreContainer One<TI, TC>() where TC : TI;
        ICoreContainer Existing<T>(T instance);
        IList<T> AllAssignable<T>();
        T Get<T>();
    }
}
