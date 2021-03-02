using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZNxt.Net.Core.Web.Models
{
    public class OAuthClient
    {
        public Client Client { get; set; }
        public string  Secret { get; set; }
    }
}
