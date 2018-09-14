namespace ZNxt.Net.Core.Interfaces
{
    public interface IModuleUninstaller
    {
        bool Uninstall(string moduleName, IHttpContextProxy httpProxy);
    }
}