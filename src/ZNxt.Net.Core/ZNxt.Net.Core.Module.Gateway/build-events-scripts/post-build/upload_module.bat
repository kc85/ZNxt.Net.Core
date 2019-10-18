
echo OFF

 set http_base_url=https://localhost:44373

rem set  http_base_url=http://gateway.znxt.fashion
rem  set http_base_url=http://gateway.s2ftechnologies.com
rem set http_base_url=http://localhost:5000
rem set http_base_url=http://s2fschool.com:806


wget -S -O uninstall_response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Gateway\",\"InstallationKey\":\"49562cfc3b17139ea01c480b9c86a2ddacb38ff1b2e9db1bf66bab7a4e3f1fb5\"}" %http_base_url%/api/moduleinstaller/uninstall


 ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.Net.Core.Module.Gateway.1.0.0-Beta00%1.nupkg

 
 wget -S -O response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Gateway\",\"Version\":\"1.0.0-Beta00%1\",\"InstallationKey\":\"49562cfc3b17139ea01c480b9c86a2ddacb38ff1b2e9db1bf66bab7a4e3f1fb5\"}" %http_base_url%/api/moduleinstaller/install
