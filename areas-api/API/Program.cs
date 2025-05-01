using API.Middlewares;
using Application.DTOs.AreaDtos;
using Application.Interfaces;
using Application.Mappers;
using Application.Mappers.AreaMappers;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.AreaInterfaces;
using Domain.Interfaces.UsuarioInterfaces;
using Infrastructure.DbContexts;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Net;
using System.Reflection;
using System.Text;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using API.Consumers;
using API.Workers;
using Domain.Interfaces;
using Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddSingleton<UsuarioAtualizadoEventConsumer>();

builder.Services.AddHostedService<ConsumerWorker>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "API"))
.WithMetrics(builder => builder
.AddMeter("AppMetrics")
.AddAspNetCoreInstrumentation()
.AddRuntimeInstrumentation()
.AddProcessInstrumentation()
.AddPrometheusExporter());


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<OnlyReadDbContext>(options =>
{
    DbContextOptionsConfigurator.Configure(options);
}, ServiceLifetime.Scoped);

builder.Services.AddDbContext<OnlyWriteDbContext>(options =>
{
    DbContextOptionsConfigurator.Configure(options);
}, ServiceLifetime.Scoped);

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();


builder.Services.AddTransient<IResponseService, ResponseService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddSingleton<IMetricsService, MetricsService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IAreaService, AreaService>();

builder.Services.AddScoped<AreaToAreaResponseMapper>();

var mapperConfig = new MapperConfiguration(cfg =>
{
    AreaToAreaResponseMapper.ConfigureMapping(cfg, builder.Services);

    cfg.CreateMap<NovaAreaRequest, Area>();
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var key = Encoding.ASCII.GetBytes(configuration.GetValue<string>("SecretJWT")!);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        x.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                return ResponseService.GetPatterResponse(
                    context.HttpContext,
                    HttpStatusCode.Unauthorized,
                    "Voce nao tem autorizacao para acessar este recurso.");
            },
            OnForbidden = context =>
            {
                return ResponseService.GetPatterResponse(
                    context.HttpContext,
                    HttpStatusCode.Forbidden,
                    "Voce nao tem permissao para acessar este recurso.");
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(x =>
{
    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
    x.IncludeXmlComments(xmlPath);

    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

//app.UseMetricServer();
//app.UseHttpMetrics();
app.UseMiddleware<MetricsMiddleware>();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionMiddleware>();

app.Run("http://0.0.0.0:8080");

public partial class Program { }
