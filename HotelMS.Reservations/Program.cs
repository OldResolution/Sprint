using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using HotelMS.Reservations.BackgroundServices;
using HotelMS.Reservations.Clients;
using HotelMS.Reservations.Data;
using HotelMS.Reservations.Exceptions;
using HotelMS.Reservations.Helpers;
using HotelMS.Reservations.Repositories.Classes;
using HotelMS.Reservations.Repositories.Interfaces;
using HotelMS.Reservations.Services.Classes;
using HotelMS.Reservations.Services.Interfaces;
using HotelMS.Reservations.Validators;
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
    .WriteTo.File("logs/reservations-service-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("ReservationDbConnection");
builder.Services.AddDbContext<ReservationDbContext>(options =>
{
    if (builder.Configuration.GetValue<bool>("UseInMemoryDatabase") || string.IsNullOrEmpty(connectionString))
        options.UseInMemoryDatabase("ReservationDB");
    else
        options.UseSqlServer(connectionString);
});

// Repositories
builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// Services
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Typed HttpClient for HotelMS.HotelManagement
var hotelServiceBaseUrl = builder.Configuration["HotelService:BaseUrl"] ?? "https://localhost:7002/";
builder.Services.AddHttpClient<IHotelClient, HotelClient>(client =>
{
    client.BaseAddress = new Uri(hotelServiceBaseUrl);
});

// Helpers
builder.Services.AddScoped<IEmailHelper, EmailHelper>();

// Background Service for Expiring Pending Reservations
builder.Services.AddHostedService<PendingReservationExpiryService>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateReservationValidator>();

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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HotelMS.Reservations API", Version = "v1" });
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
    var ctx = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();
    DbInitializer.Initialize(ctx);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
