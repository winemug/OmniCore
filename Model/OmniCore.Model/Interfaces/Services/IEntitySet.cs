using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Entities;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IEntitySet<T> where T : class, IEntity
    {
    }
}
