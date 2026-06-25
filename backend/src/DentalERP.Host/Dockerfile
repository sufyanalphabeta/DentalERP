FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
# Install Arabic/Unicode fonts for QuestPDF server-side PDF generation
RUN apt-get update \
 && apt-get install -y --no-install-recommends fonts-noto-core fontconfig \
 && fc-cache -fv \
 && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all project files first (for layer caching)
COPY ["src/DentalERP.Host/DentalERP.Host.csproj",                       "src/DentalERP.Host/"]
COPY ["src/DentalERP.SharedKernel/DentalERP.SharedKernel.csproj",       "src/DentalERP.SharedKernel/"]
COPY ["src/DentalERP.Modules.IAM/DentalERP.Modules.IAM.csproj",         "src/DentalERP.Modules.IAM/"]
COPY ["src/DentalERP.Modules.Patients/DentalERP.Modules.Patients.csproj","src/DentalERP.Modules.Patients/"]
COPY ["src/DentalERP.Modules.Clinical/DentalERP.Modules.Clinical.csproj","src/DentalERP.Modules.Clinical/"]
COPY ["src/DentalERP.Modules.Financial/DentalERP.Modules.Financial.csproj","src/DentalERP.Modules.Financial/"]
COPY ["src/DentalERP.Modules.Laboratory/DentalERP.Modules.Laboratory.csproj","src/DentalERP.Modules.Laboratory/"]
COPY ["src/DentalERP.Modules.Radiology/DentalERP.Modules.Radiology.csproj","src/DentalERP.Modules.Radiology/"]
COPY ["src/DentalERP.Modules.Inventory/DentalERP.Modules.Inventory.csproj","src/DentalERP.Modules.Inventory/"]
COPY ["src/DentalERP.Modules.Purchasing/DentalERP.Modules.Purchasing.csproj","src/DentalERP.Modules.Purchasing/"]
COPY ["src/DentalERP.Modules.Expenses/DentalERP.Modules.Expenses.csproj","src/DentalERP.Modules.Expenses/"]
COPY ["src/DentalERP.Modules.Assets/DentalERP.Modules.Assets.csproj",   "src/DentalERP.Modules.Assets/"]

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
