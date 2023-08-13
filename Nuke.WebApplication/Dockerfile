FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Nuke.WebApplication/Nuke.WebApplication.csproj", "Nuke.WebApplication/"]
RUN dotnet restore "Nuke.WebApplication/Nuke.WebApplication.csproj"
COPY . .
WORKDIR "/src/Nuke.WebApplication"
RUN dotnet build "Nuke.WebApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nuke.WebApplication.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nuke.WebApplication.dll"]
