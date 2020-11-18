FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base

RUN apt-get update
RUN apt-get install -y apt-utils
RUN apt-get install -y libgdiplus
RUN apt-get install -y libc6-dev 
RUN ln -s /usr/lib/libgdiplus.so/usr/lib/gdiplus.dll

WORKDIR /app
EXPOSE 3496

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ZNxt.Net.Core.Web.Sample/ZNxt.Net.Core.Web.Sample.csproj ZNxt.Net.Core.Web.Sample/
COPY ZNxt.Net.Core.DB.Mongo/ZNxt.Net.Core.DB.Mongo.csproj ZNxt.Net.Core.DB.Mongo/
COPY ZNxt.Net.Core/ZNxt.Net.Core.csproj ZNxt.Net.Core/
COPY ZNxt.Net.Core.Web/ZNxt.Net.Core.Web.csproj ZNxt.Net.Core.Web/
COPY ZNxt.Net.Core.Web.ContentHandler/ZNxt.Net.Core.Web.ContentHandler.csproj ZNxt.Net.Core.Web.ContentHandler/

RUN dotnet restore ZNxt.Net.Core.Web.Sample/ZNxt.Net.Core.Web.Sample.csproj
COPY . .

WORKDIR /src/ZNxt.Net.Core.Web.Sample
RUN dotnet build ZNxt.Net.Core.Web.Sample.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ZNxt.Net.Core.Web.Sample.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ZNxt.Net.Core.Web.Sample.dll"]
