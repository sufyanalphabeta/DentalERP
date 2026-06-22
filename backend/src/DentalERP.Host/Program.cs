using System.Text;
using DentalERP.Modules.Clinical;
using DentalERP.Modules.Financial;
using DentalERP.Modules.IAM;
using DentalERP.Modules.Inventory;
using DentalERP.Modules.Laboratory;
using DentalERP.Modules.Patients;
using DentalERP.Modules.Assets;
using DentalERP.Modules.Expenses;
using DentalERP.Modules.Purchasing;
using DentalERP.Modules.Radiology;
using DentalERP.SharedKernel.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console()
       .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey is required.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Shared Kernel Pipeline Behaviors
builder.Services.AddSharedKernel();

// Modules
builder.Services.AddIAMModule(builder.Configuration);
builder.Services.AddPatientsModule(builder.Configuration);
builder.Services.AddClinicalModule(builder.Configuration);
builder.Services.AddFinancialModule(builder.Configuration);
builder.Services.AddLaboratoryModule(builder.Configuration);
builder.Services.AddRadiologyModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);
builder.Services.AddPurchasingModule(builder.Configuration);
builder.Services.AddExpensesModule(builder.Configuration);
builder.Services.AddAssetsModule(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "DentalERP API", Version = "v1" });
    c.CustomSchemaIds(type => type.FullName!.Replace('+', '.'));
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            []
        }
    });
});

// CORS — LAN access only
builder.Services.AddCors(opts =>
    opts.AddPolicy("LAN", p => p
        .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"])
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseCors("LAN");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DentalERP v1"));
}

app.UseAuthentication();
app.UseAuthorization();

// Module Endpoints
app.MapIAMEndpoints();
app.MapPatientsModule();
app.MapClinicalModule();
app.MapFinancialModule();
app.MapLaboratoryModule();
app.MapRadiologyModule();
app.MapInventoryModule();
app.MapPurchasingModule();
app.MapExpensesModule();
app.MapAssetsModule();

// Health check
app.MapGet("/health", () => Results.Text($"Healthy|{DateTime.UtcNow:O}"))
    .AllowAnonymous()
    .WithTags("System");

app.Run();

public partial class Program { }
