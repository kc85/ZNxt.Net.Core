using System.Collections.Generic;
using ZNxt.Net.Core.Enums;

namespace ZNxt.Net.Core.Config
{
    public class AppInstallerConfig
    {
        public AppInstallType InstallType;

        public string Name { get; set; }

        public string AdminAccount { get; set; }

        public string AdminPassword { get; set; }

        public List<string> DefaultModules { get; set; }

        public AppInstallerConfig()
        {
            DefaultModules = new List<string>();
        }
    }
    
}