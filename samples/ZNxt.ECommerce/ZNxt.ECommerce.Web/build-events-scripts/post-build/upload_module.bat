
echo OFF
set http_base_url=https://localhost:44373

rem set http_base_url=https://ZNxt.App



 ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.ECommerce.Web.1.0.0-Beta00%1.nupkg

 wget -S -O response.json --post-data "{\"Name\":\"ZNxt.ECommerce.Web\",\"Version\":\"1.0.0-Beta00%1\"}" %http_base_url%/api/moduleinstaller/install
