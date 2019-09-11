
echo OFF
 set http_base_url=https://localhost:44373

rem set  http_base_url=http://znxt.fashion/

wget -S -O uninstall_response.json --post-data "{\"Name\":\"ZNxt.Module.Identity\"}" %http_base_url%/api/moduleinstaller/uninstall


 ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.Module.Identity.1.0.0-Beta00%1.nupkg

 
 wget -S -O response.json --post-data "{\"Name\":\"ZNxt.Module.Identity\",\"Version\":\"1.0.0-Beta00%1\"}" %http_base_url%/api/moduleinstaller/install
