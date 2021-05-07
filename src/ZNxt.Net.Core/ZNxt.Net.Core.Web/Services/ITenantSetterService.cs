using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services
{
    public interface ITenantSetterService
    {
        void SetTenant(UserModel userModel);

        bool AddUserToTenant(UserModel userModel);
    }
}
