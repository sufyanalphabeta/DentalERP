FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/DentalERP.Host/DentalERP.Host.csproj", "src/DentalERP.Host/"]
COPY ["src/DentalERP.SharedKernel/DentalERP.SharedKernel.csproj", "src/DentalERP.SharedKernel/"]
COPY ["src/DentalERP.Modules.IAM/DentalERP.Modules.IAM.csproj", "src/DentalERP.Modules.IAM/"]
RUN dotnet restore "src/DentalERP.Host/DentalERP.Host.csproj"
COPY . .
WORKDIR "/src/src/DentalERP.Host"
RUN dotnet build "DentalERP.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DentalERP.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DentalERP.Host.dll"]
