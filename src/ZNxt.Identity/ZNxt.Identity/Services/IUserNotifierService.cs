using System.Threading.Tasks;
using ZNxt.Net.Core.Model;

namespace ZNxt.Identity.Services
{
    public interface IUserNotifierService
    {
        Task<bool> SendWelcomeEmailAsync(UserModel user);
    }
}