
echo OFF


set http_base_url=https://znxt.app


wget -S -O uninstall_response.json --post-data "{\"Name\":\"ZNxt.Module.MyModule1\",\"InstallationKey\":\"f5d2ce5ed463e844266feb92abba804244a7b5287484ecd2041395dad4bf18c3\"}" %http_base_url%/api/moduleinstaller/uninstall

ZNxtApp.CLI -u   %http_base_url%/api/moduleinstaller/upload  .\..\..\bin\Debug\ZNxt.Module.MyModule1.1.0.0-Beta00%1.nupkg

wget -S -O response.json --post-data "{\"Name\":\"ZNxt.Module.MyModule1\",\"Version\":\"1.0.0-Beta00%1\",\"InstallationKey\":\"f5d2ce5ed463e844266feb92abba804244a7b5287484ecd2041395dad4bf18c3\"}" %http_base_url%/api/moduleinstaller/install
