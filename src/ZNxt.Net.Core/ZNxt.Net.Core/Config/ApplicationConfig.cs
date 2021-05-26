using System;
using System.IO;
using ZNxt.Net.Core.Enums;
using ZNxt.Net.Core.Helpers;

namespace ZNxt.Net.Core.Config
{
    /// <summary>
    /// Get Application name from config
    /// </summary>
    public static class ApplicationConfig
    {

        public static AppInstallStatus AppInstallStatus
        { get; set; }
        public static string AppName
        {
            get
            {
                return CommonUtility.GetAppConfigValue("AppName");
            }
        }

        /// <summary>
        /// Get Application id from Config
        /// </summary>
        public static string AppID
        {
            get
            {
                return CommonUtility.GetAppConfigValue("AppId");
            }
        }

        /// <summary>
        /// Get MongoDB Connection String from Config
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return CommonUtility.GetAppConfigValue("ConnectionString");
            }
        }


        /// <summary>
        /// Get Mongo DB Name 
        /// </summary>
        public static string DataBaseName
        {
            get
            {
                return CommonUtility.GetAppConfigValue("DataBaseName");
            }
        }

        /// <summary>
        /// Application path in App in not on root folder of IIS WebSite 
        /// </summary>
        public static string AppPath
        {
            get
            {
                return CommonUtility.GetAppConfigValue("AppPath");
            }
        }

        /// <summary>
        /// Application Backend base URL 
        /// </summary>
        public static string AppBackendPath
        {
            get
            {
                return (CommonUtility.GetAppConfigValue("BackendPath") == null ?
                    "/admin001" : CommonUtility.GetAppConfigValue("BackendPath"));
            }
        }

        /// <summary>
        /// Application Default Page
        /// </summary>
        public static string AppDefaultPage
        {
            get
            {
                return CommonUtility.GetAppConfigValue("DefaultPage") == null ? "/index.z" : CommonUtility.GetAppConfigValue("DefaultPage");
            }
        }

        /// <summary>
        /// System Temp Folder 
        /// </summary>
        public static string SystemTempFolder
        {
            get
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + AppName;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        /// <summary>
        ///  Applicaton Temp Folder
        /// </summary>
        public static string AppTempFolderPath
        {
            get
            {
                var tempFolder = string.Format("{0}\\{1}", AppInstallFolder, AppID);

                return tempFolder;
            }
        }


        /// <summary>
        /// Application running mode 
        /// </summary>
        public static ApplicationMode GetApplicationMode
        {
            get
            {
                string appMode = CommonUtility.GetAppConfigValue("AppMode");
                ApplicationMode appModeEnum = ApplicationMode.Maintenance;
                Enum.TryParse<ApplicationMode>(appMode, out appModeEnum);
                return appModeEnum;
            }
        }


        /// <summary>
        /// Application Module Cache Path
        /// </summary>
        public static string AppModulePath
        {
            get
            {
                return CommonUtility.GetAppConfigValue("ModuleCachePath");
            }
        }

        /// <summary>
        /// Applicaton  Install Folder 
        /// </summary>
        public static string AppInstallFolder
        {
            get
            {
                return CommonUtility.GetAppConfigValue("ModuleCachePath");
            }
        }

        /// <summary>
        /// Static content cache flag
        /// </summary>
        public static bool StaticContentCache
        {
            get
            {
                bool.TryParse(CommonUtility.GetAppConfigValue("StaticContentCache"), out bool result);
                return result;
            }
        }

        //public static string AuthToken { get; set; }

        private static double _sessionDuration = 30;

        public static double SessionDuration { get { return _sessionDuration; } set { _sessionDuration = value; } }

        public static string AppBinPath { get { return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); } }

        public static string AppWWWRootPath => string.IsNullOrEmpty(CommonUtility.GetAppConfigValue("WWWRootFolder"))  ?  "/wwwroot" : CommonUtility.GetAppConfigValue("WWWRootFolder");

        public static string AppEndpoint => CommonUtility.GetAppConfigValue("AppEndpoint");

        public static string ApiGatewayEndpoint => CommonUtility.GetAppConfigValue("ApiGatewayEndpoint");
        public static string SSOEndpoint => CommonUtility.GetAppConfigValue("SSOUrl");
        public static bool IsSSO {
            get {
                return CommonUtility.GetAppConfigValue("IsSSO") == "true";
             }
        }
        public static int HttpPort
        {
            get
            {
                int port = 80;
                if(!int.TryParse(CommonUtility.GetAppConfigValue("HttpPort"), out port))
                {
                    port = 80;
                }
                return port;
            }
        }
        public static int HttpsPort
        {
            get
            {
                int port = 443;
                if(!int.TryParse(CommonUtility.GetAppConfigValue("HttpsPort"), out port))
                {
                    port = 443;
                }
                return port;
            }
        }

    }
}