
using Microsoft.EntityFrameworkCore;
using PruebaTecnica_MonoLegal.Context;
using PruebaTecnica_MonoLegal.Repositories;
using PruebaTecnica_MonoLegal.Repositories.Interfaces;
using PruebaTecnica_MonoLegal.Services;
using PruebaTecnica_MonoLegal.Services.Interfaces;
using PruebaTecnica_MonoLegal.Configuration;
using PruebaTecnica_MonoLegal.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controladores
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new ObjectIdJsonConverter()
        );
    });

// permitir peticiones de frontend
var frontendUrl = builder.Configuration["FrontendUrl"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(frontendUrl!)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Repositorios y servicios de facturas
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();

// Autenticación de usuarios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Notificaciones y configuración de recordatorios
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IReminderSettingsRepository, ReminderSettingsRepository>();

// Servicio de correo
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();

// Recordatorios automáticos
builder.Services.AddSingleton<ReminderSignal>();
builder.Services.AddHostedService<ReminderBackgroundService>();

// MongoDB Atlas
var connectionString =
    builder.Configuration.GetConnectionString("MongoDbUri")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión MongoDbUri.");

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseMongoDB(connectionString, "BDPruebaTecnica"));

// Configuración JWT
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "No se encontró la configuración JwtSettings.");

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
{
    throw new InvalidOperationException(
        "No se configuró la clave secreta del JWT.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })

.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var tokenId = context.Principal?.FindFirst(
                System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrWhiteSpace(tokenId))
            {
                context.Fail("El token no contiene un identificador válido.");
                return Task.CompletedTask;
            }

            var revokedTokenService = context.HttpContext.RequestServices
                .GetRequiredService<IRevokedTokenService>();

            if (revokedTokenService.IsTokenRevoked(tokenId))
            {
                context.Fail("El token ha sido revocado.");
            }

            return Task.CompletedTask;
        }
    };
});


builder.Services.AddSingleton<IRevokedTokenService, RevokedTokenService>();

builder.Services.AddAuthorization();

var app = builder.Build();


app.UseHttpsRedirection();
app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
