using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Services
{
    public class UserService : IUserService
    {
        public IAsyncEnumerable<IUser> GetUsers()
        {
            throw new System.NotImplementedException();
        }

        public Task<IUser> FindUser(string userName)
        {
            throw new System.NotImplementedException();
        }

        public Task<IUser> AddNewUser(string userName)
        {
            throw new System.NotImplementedException();
        }

        public Task<IUser> RemoveUser(string userName)
        {
            throw new System.NotImplementedException();
        }
    }
}