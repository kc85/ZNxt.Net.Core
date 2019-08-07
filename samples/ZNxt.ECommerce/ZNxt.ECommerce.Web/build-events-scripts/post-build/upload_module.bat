
echo OFF
rem set http_base_url=https://localhost:44373

set http_base_url=http://ec2-13-233-76-129.ap-south-1.compute.amazonaws.com



wget -S -O uninstall_response.json --post-data "{\"Name\":\"ZNxt.ECommerce.Web\"}" %http_base_url%/api/moduleinstaller/uninstall

 ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.ECommerce.Web.1.0.0-Beta00%1.nupkg

 
 wget -S -O install_response.json --post-data "{\"Name\":\"ZNxt.ECommerce.Web\",\"Version\":\"1.0.0-Beta00%1\"}" %http_base_url%/api/moduleinstaller/install
