FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BirFikrimVar.Web/BirFikrimVar.Web.csproj", "BirFikrimVar.Web/"]
COPY ["BirFikrimVar.Core/BirFikrimVar.Core.csproj", "BirFikrimVar.Core/"]
COPY ["BirFikrimVar.Service/BirFikrimVar.Service.csproj", "BirFikrimVar.Service/"]

RUN dotnet restore "BirFikrimVar.Web/BirFikrimVar.Web.csproj"

COPY . .

WORKDIR "/src/BirFikrimVar.Web"
RUN dotnet publish "BirFikrimVar.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BirFikrimVar.Web.dll"]
