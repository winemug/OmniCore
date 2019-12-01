using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Model.Interfaces.Services
{
    public interface IUserService
    {
        IAsyncEnumerable<IUser> GetUsers();
        Task<IUser> FindUser(string userName);
        Task<IUser> AddNewUser(string userName);
        Task<IUser> RemoveUser(string userName);
    }
}