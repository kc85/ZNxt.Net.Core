using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services
{
    public class TenantSetterService : ITenantSetterService
    {
        public void SetTenant(UserModel userModel)
        {
            userModel.tenants = new List<TenantModel>();
        }
    }
}
