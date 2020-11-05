
echo OFF
rem set http_base_url=https://localhost:44373

rem set  http_base_url=http://admin.znxt.fashion

 rem set  http_base_url=http://admin.s2ftechnologies.com
 rem set  http_base_url=http://sqa.admin.s2fschool.com

 set  http_base_url=http://qa-admin.ellummullum.com

rem wget -S -O uninstall_response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Admin\"}" %http_base_url%/api/moduleinstaller/uninstall


 rem ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.Net.Core.Module.Admin.1.0.0-Beta00%1.nupkg

 
 rem wget -S -O response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Admin\",\"Version\":\"1.0.0-Beta00%1\"}" %http_base_url%/api/moduleinstaller/install

 wget -S -O uninstall_response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Admin\",\"InstallationKey\":\"f5d2ce5ed463e844266feb92abba804244a7b5287484ecd2041395dad4bf18c3\"}" %http_base_url%/api/moduleinstaller/uninstall


 ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.Net.Core.Module.Admin.1.0.0-Beta00%1.nupkg

 
 wget -S -O response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Admin\",\"Version\":\"1.0.0-Beta00%1\",\"InstallationKey\":\"f5d2ce5ed463e844266feb92abba804244a7b5287484ecd2041395dad4bf18c3\"}" %http_base_url%/api/moduleinstaller/install
