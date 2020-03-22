using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Services.Configuration
{
    public class User : IUser
    {
        public UserEntity Entity { get; set; }
    }
}
