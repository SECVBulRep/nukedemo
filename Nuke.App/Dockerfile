FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Nuke.App/Nuke.App.csproj", "Nuke.App/"]
RUN dotnet restore "Nuke.App/Nuke.App.csproj"
COPY . .
WORKDIR "/src/Nuke.App"
RUN dotnet build "Nuke.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nuke.App.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nuke.App.dll"]
