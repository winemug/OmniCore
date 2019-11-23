using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces.Entities
{
    public interface IUserEntity : IUserAttributes, IEntity
    {
        bool ManagedRemotely { get; set; }
        IList<IRadioEntity> Radios { get; }

    }
}
