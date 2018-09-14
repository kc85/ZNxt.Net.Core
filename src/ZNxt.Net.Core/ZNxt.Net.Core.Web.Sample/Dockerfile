FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 3596
EXPOSE 44373

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ZNxt.Net.Core.Web.Sample/ZNxt.Net.Core.Web.Sample.csproj ZNxt.Net.Core.Web.Sample/
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
