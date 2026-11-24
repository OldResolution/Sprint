using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using HotelMS.Operations.Clients;
using HotelMS.Operations.Data;
using HotelMS.Operations.Exceptions;
using HotelMS.Operations.Repositories.Classes;
using HotelMS.Operations.Repositories.Interfaces;
using HotelMS.Operations.Services.Classes;
using HotelMS.Operations.Services.Interfaces;
using HotelMS.Operations.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/operations-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("OperationsDbConnection");
builder.Services.AddDbContext<OperationsDbContext>(options =>
{
    if (builder.Configuration.GetValue<bool>("UseInMemoryDatabase") || string.IsNullOrEmpty(connectionString))
        options.UseInMemoryDatabase("OperationsDB");
    else
        options.UseSqlServer(connectionString);
});

// Repositories
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();

// Services
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Typed HttpClient for HotelMS.Reservations
var resServiceBaseUrl = builder.Configuration["ReservationService:BaseUrl"] ?? "https://localhost:7003/";
builder.Services.AddHttpClient<IReservationClient, ReservationClient>(client =>
{
    client.BaseAddress = new Uri(resServiceBaseUrl);
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateDepartmentValidator>();

builder.Services.AddControllers();

// JWT Authentication
var secret = builder.Configuration["JwtSettings:Secret"] ?? "HotelManagementSystemSecretKey2026_SecureMinimum32CharsLength!";
var issuer = builder.Configuration["JwtSettings:Issuer"] ?? "HotelMS";
var audience = builder.Configuration["JwtSettings:Audience"] ?? "HotelMSClients";
var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HotelMS.Operations API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Correlation ID middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<OperationsDbContext>();
    DbInitializer.Initialize(ctx);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
