
echo OFF
 set http_base_url=https://localhost:44373

rem set  http_base_url=http://ec2-13-233-144-71.ap-south-1.compute.amazonaws.com

wget -S -O uninstall_response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Base\"}" %http_base_url%/api/moduleinstaller/uninstall


 ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.Net.Core.Module.Base.1.0.0-Beta00%1.nupkg

 
 wget -S -O response.json --post-data "{\"Name\":\"ZNxt.Net.Core.Module.Base\",\"Version\":\"1.0.0-Beta00%1\"}" %http_base_url%/api/moduleinstaller/install
