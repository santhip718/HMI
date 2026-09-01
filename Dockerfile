FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY VMC/backend/src/VmcHmi.Api/VmcHmi.Api.csproj VMC/backend/src/VmcHmi.Api/
COPY VMC/backend/src/VmcHmi.Infrastructure/VmcHmi.Infrastructure.csproj VMC/backend/src/VmcHmi.Infrastructure/
COPY VMC/backend/src/VmcHmi.Application/VmcHmi.Application.csproj VMC/backend/src/VmcHmi.Application/
COPY VMC/backend/src/VmcHmi.Domain/VmcHmi.Domain.csproj VMC/backend/src/VmcHmi.Domain/

RUN dotnet restore VMC/backend/src/VmcHmi.Api/VmcHmi.Api.csproj

COPY VMC/backend/ VMC/backend/
WORKDIR /src/VMC/backend/src/VmcHmi.Api
RUN dotnet publish "VmcHmi.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 10000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VmcHmi.Api.dll"]
