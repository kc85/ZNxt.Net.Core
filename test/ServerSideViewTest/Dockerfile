FROM microsoft/dotnet:2.2-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ServerSideViewTest/ServerSideViewTest.csproj ServerSideViewTest/
RUN dotnet restore ServerSideViewTest.csproj
COPY . .
WORKDIR /src/ServerSideViewTest
RUN dotnet build ServerSideViewTest.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ServerSideViewTest.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ServerSideViewTest.dll"]
