using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IUserNotifierService
    {
        Task<bool> SendWelcomeEmailAsync(UserModel user);
        Task<bool> SendWelcomeEmailWithOTPLoginAsync(UserModel user);
    }
}
