using ZNxt.Net.Core.Enums;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IAppInstaller
    {
        void Install(IHttpContextProxy httpProxy);

        AppInstallStatus Status { get; }
    }
}