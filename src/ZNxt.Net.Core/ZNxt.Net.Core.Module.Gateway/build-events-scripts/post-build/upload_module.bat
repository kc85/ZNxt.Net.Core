
echo OFF

rem set http_base_url=https://localhost:44373

rem set  http_base_url=http://gateway.znxt.fashion
rem  set http_base_url=http://gateway.s2ftechnologies.com
rem set http_base_url=http://localhost:5000
rem set http_base_url=http://s2fschool.com:806
rem set http_base_url=http://sqa.gateway.s2fschool.com

 set http_base_url=http://qa-gateway.ellummullum.com

rem wget -S -O uninstall_response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Gateway\",\"InstallationKey\":\"49562cfc3b17139ea01c480b9c86a2ddacb38ff1b2e9db1bf66bab7a4e3f1fb5\"}" %http_base_url%/api/moduleinstaller/uninstall

rem ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.Net.Core.Module.Gateway.1.0.0-Beta00%1.nupkg

rem wget -S -O response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Gateway\",\"Version\":\"1.0.0-Beta00%1\",\"InstallationKey\":\"49562cfc3b17139ea01c480b9c86a2ddacb38ff1b2e9db1bf66bab7a4e3f1fb5\"}" %http_base_url%/api/moduleinstaller/install

wget -S -O uninstall_response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Gateway\",\"InstallationKey\":\"f5d2ce5ed463e844266feb92abba804244a7b5287484ecd2041395dad4bf18c3\"}" %http_base_url%/api/moduleinstaller/uninstall

ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.Net.Core.Module.Gateway.1.0.0-Beta00%1.nupkg

wget -S -O response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Gateway\",\"Version\":\"1.0.0-Beta00%1\",\"InstallationKey\":\"f5d2ce5ed463e844266feb92abba804244a7b5287484ecd2041395dad4bf18c3\"}" %http_base_url%/api/moduleinstaller/install
