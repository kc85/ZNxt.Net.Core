using System.Collections.Generic;
using System.Threading.Tasks;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IUserNotifierService
    {
        Task<bool> SendWelcomeEmailAsync(UserModel user, string message= null);
        Task<bool> SendWelcomeEmailWithOTPLoginAsync(UserModel user, string message = null);

        Task<bool> SendForgetpasswordEmailWithOTPAsync(UserModel user, string message = null);
        Task<bool> SendForgetUsernamesEmailAsync(List<UserModel> user, string message = null);

        Task<bool> SendMobileAuthRegistrationOTPAsync(MobileAuthRegisterResponse  mobileAuth, string message = null);

    }
}
