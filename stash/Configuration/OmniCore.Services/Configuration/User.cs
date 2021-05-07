using OmniCore.Model.Entities;
using OmniCore.Model.Interfaces.Services;

namespace OmniCore.Services.Configuration
{
    public class User : IUser
    {
        public UserEntity Entity { get; set; }
    }
}