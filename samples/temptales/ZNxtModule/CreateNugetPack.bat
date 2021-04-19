
rd /s /q .\ZNxt.Module.MyModule1\content\.vs

rd /s /q .\ZNxt.Module.MyModule1\content\ZNxt.Module.MyModule1\.vs
rd /s /q .\ZNxt.Module.MyModule1\content\ZNxt.Module.MyModule1\bin
rd /s /q .\ZNxt.Module.MyModule1\content\ZNxt.Module.MyModule1\obj

rd /s /q .\ZNxt.Module.MyModule1\content\ZNxt.Module.MyModule1.Test\.vs
rd /s /q .\ZNxt.Module.MyModule1\content\ZNxt.Module.MyModule1.Test\bin
rd /s /q .\ZNxt.Module.MyModule1\content\ZNxt.Module.MyModule1.Test\obj

rd /s /q .\ZNxt.Module.MyModule1\content\ZNxt.Module.MyModule1.Web.Test\.vs
rd /s /q .\ZNxt.Module.MyModule1\content\ZNxt.Module.MyModule1.Web.Test\bin
rd /s /q .\ZNxt.Module.MyModule1\content\ZNxt.Module.MyModule1.Web.Test\obj


nuget pack .\ZNxt.Module.MyModule1\ZNxt.Module.Template.CSharp.nuspec
