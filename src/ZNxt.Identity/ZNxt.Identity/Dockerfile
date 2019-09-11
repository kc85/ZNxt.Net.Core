FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["ZNxt.Identity.csproj", ""]
RUN dotnet restore "./ZNxt.Identity.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ZNxt.Identity.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ZNxt.Identity.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ZNxt.Identity.dll"]