﻿
echo OFF

rem set http_base_url=https://localhost:44373

rem set  http_base_url=http://gateway.znxt.fashion
rem  set http_base_url=http://gateway.s2ftechnologies.com
rem set http_base_url=http://localhost:5000
set http_base_url=http://s2fschool.com:806


wget -S -O uninstall_response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Gateway\"}" %http_base_url%/api/moduleinstaller/uninstall


 ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.Net.Core.Module.Gateway.1.0.0-Beta00%1.nupkg

 
 wget -S -O response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Gateway\",\"Version\":\"1.0.0-Beta00%1\"}" %http_base_url%/api/moduleinstaller/install
