using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Enums;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Web.Helper;
using ZNxt.Net.Core.Web.Services.Api.Installer.Model;

namespace ZNxt.Net.Core.Web.Services.Api.Installer
{
    public class AppInstallerContoller
    {
        private readonly IDBService _dbService;
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly IDBServiceConfig _dbConfig;
        private readonly IServiceResolver _serviceResolver;
        public AppInstallerContoller(IDBService dbService,IServiceResolver serviceResolver, IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy,IDBServiceConfig dbConfig)
        {
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _dbConfig = dbConfig;
            _serviceResolver = serviceResolver;

        }
        [Route("/appinstaller/status", CommonConst.ActionMethods.GET)]
        public JObject Get()
        {
            var response = new JObject
            {
                [CommonConst.CommonField.STATUS] = ApplicationConfig.AppInstallStatus.ToString()
            };
            return _responseBuilder.Success(response);
        }

        [Route("/appinstaller/setdb", CommonConst.ActionMethods.POST)]
        public JObject SetDb()
        {
            var data =  _httpContextProxy.GetRequestBody<DBConnection>();
            _dbConfig.Set(data.Database, data.ConnectionString);
            ApplicationConfig.AppInstallStatus = AppInstallStatus.Init; 
            var db = _serviceResolver.Resolve<IDBService>();
            if (db.IsConnected)
            {
                _dbConfig.Save();
                return _responseBuilder.Success();
            }
            else
            {
                ApplicationConfig.AppInstallStatus = AppInstallStatus.DBNotSet;
                return _responseBuilder.ServerError(new JObject
                {
                    [CommonConst.CommonField.ERR_MESSAGE] = "DB Connection error"
                });
            }
        }
        //[Route("/appinstaller/apprestart", CommonConst.ActionMethods.GET)]
        //public JObject AppRestart()
        //{
        //    IApplicationLifetime appLifetime = _serviceResolver.Resolve<IApplicationLifetime>();
        //    appLifetime.StopApplication();
        //    return _responseBuilder.Success();
        //}

        [Route("/appinstaller/install", CommonConst.ActionMethods.POST)]
        public JObject Install()
        {
            var data = _httpContextProxy.GetRequestBody<AppIntallModel>();
            if (data.Password.Length == 0 && data.Email.Length == 0)
            {
                return _responseBuilder.BadRequest();
            }
            ApplicationConfig.AppInstallStatus = AppInstallStatus.Inprogress;
            var db = _serviceResolver.Resolve<IDBService>();
            var userAccontHelper = _serviceResolver.Resolve<UserAccontHelper>();
            if (db.IsConnected)
            {
                var appSetting = _serviceResolver.Resolve<IAppSettingService>();
                var customConfig = userAccontHelper.CreateNewUserObject(data.Email, data.Email, data.Email, data.Password, UserIDType.Email);
                userAccontHelper.AddClaim(customConfig, CommonConst.CommonField.ROLE_CLAIM_TYPE, CommonConst.CommonField.SYS_ADMIN_ROLE);
                if (db.Write(CommonConst.Collection.USERS, customConfig))
                {
                    ApplicationConfig.AppInstallStatus = AppInstallStatus.Finish;
                    appSetting.SetAppSetting(CommonConst.CommonValue.APPINSTALLSTATUS, AppInstallStatus.Finish.ToString());
                    return _responseBuilder.Success();
                }
                else
                {
                    ApplicationConfig.AppInstallStatus = AppInstallStatus.Init;
                    return _responseBuilder.ServerError();
                }
            }
            else
            {
                ApplicationConfig.AppInstallStatus = AppInstallStatus.DBNotSet;
                return _responseBuilder.ServerError(new JObject
                {
                    [CommonConst.CommonField.ERR_MESSAGE] = "DB Connection error"
                });
            }
        }


    }
}
