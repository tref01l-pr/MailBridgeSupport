using System.Text.Json.Serialization;
using MailBridgeSupport.Application.Services;
using MailBridgeSupport.DataAccess.SqlServer;
using MailBridgeSupport.DataAccess.SqlServer.Entities;
using MailBridgeSupport.DataAccess.SqlServer.Repositories;
using MailBridgeSupport.Domain.Interfaces;
using MailBridgeSupport.Domain.Interfaces.Application;
using MailBridgeSupport.Domain.Interfaces.DataAccess;
using MailBridgeSupport.Domain.Interfaces.Infrastructure;
using MailBridgeSupport.Domain.Options;
using MailBridgeSupport.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace MailBridgeSupport.API;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JWTSecretOptions>(
            builder.Configuration.GetSection(JWTSecretOptions.JWTSecret));
        builder.Services.Configure<SmtpOptions>(
            builder.Configuration.GetSection(SmtpOptions.Smtp));
        builder.Services.Configure<ImapOptions>(
            builder.Configuration.GetSection(ImapOptions.Imap));

        builder.Services.AddControllers().AddJsonOptions(o =>
            o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWTSecret:Secret").Value!)),
            };
        });
        
        builder.Services.AddAuthorization();
        
        builder.Services.AddDetection();
        
        builder.Services.AddScoped<IImapService, ImapService>();
        builder.Services.AddScoped<ISmtpService, SmtpService>();
        builder.Services.AddScoped<ISentMessagesRepository, SentMessagesRepository>();
        builder.Services.AddScoped<ISessionsRepository, SessionsRepository>();
        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<IClientMessagesService, ClientMessagesService>();
        builder.Services.AddScoped<IModeratorsService, ModeratorsService>();
        builder.Services.AddScoped<ISentMessagesService, SentMessagesService>();
        builder.Services.AddScoped<ISystemAdminsService, SystemAdminsService>();
        
        builder.Services
            .AddIdentityCore<UserEntity>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<MailBridgeSupportDbContext>();
        
        builder.Services.AddAutoMapper(config =>
        {
            config.AddProfile<ApiMappingProfile>();
            config.AddProfile<DataAccessMappingProfile>();
        });
        
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Any", corsPolicyBuilder =>
            {
                corsPolicyBuilder
                    .WithOrigins(new [] {"http://localhost:5173", "https://localhost:5173", "http://localhost:5174"}) 
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); 
            });

        });
        
        builder.Services.AddDbContext<MailBridgeSupportDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("MailBridgeSupportDbContext"),
                x => x.MigrationsAssembly("MailBridgeSupport.DataAccess.SqlServer")));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseCors("Any");

        app.MapControllers();

        app.Run();
    }
}