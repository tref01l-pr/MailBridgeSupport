namespace MailBridgeSupport.API;

using System.Text;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JWTSecretOptions>(
            builder.Configuration.GetSection(JWTSecretOptions.JWTSecret));
        builder.Services.Configure<SmtpOptions>(
            builder.Configuration.GetSection(SmtpOptions.Smtp));
        builder.Services.Configure<ImapOptions>(
            builder.Configuration.GetSection(ImapOptions.Imap));
        builder.Services.Configure<GoogleApiOptions>(
            builder.Configuration.GetSection(GoogleApiOptions.GoogleApi));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        //builder.Services.AddHttpContextAccessor();

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

        builder.Services.AddDbContext<MailBridgeSupportDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("MailBridgeSupportDbContext"),
                x => x.MigrationsAssembly("MailBridgeSupport.DataAccess.SqlServer")));


        builder.Services
            .AddIdentity<UserEntity, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<MailBridgeSupportDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        });

        builder.Services.AddAutoMapper(config =>
        {
            config.AddProfile<ApiMappingProfile>();
            config.AddProfile<DataAccessMappingProfile>();
        });

        builder.Services.AddTransient<Seed>();
        builder.Services.AddScoped<IImapService, ImapService>();
        builder.Services.AddScoped<ISmtpService, SmtpService>();
        builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();
        builder.Services.AddScoped<ISentMessagesRepository, SentMessagesRepository>();
        builder.Services.AddScoped<IReceivedMessagesRepository, ReceivedMessagesRepository>();
        builder.Services.AddScoped<ISessionsRepository, SessionsRepository>();
        builder.Services.AddScoped<IUsersRepository, UsersRepository>();
        builder.Services.AddScoped<ICacheRepository, CacheRepository>();
        builder.Services.AddScoped<IClientMessagesService, ClientsService>();
        builder.Services.AddScoped<IMessagesService, MessagesService>();
        builder.Services.AddScoped<ISystemAdminsService, SystemAdminsService>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWTSecret:Secret").Value!)),
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddStackExchangeRedisCache(redisOptions =>
        {
            string? connection = builder.Configuration
                .GetConnectionString("Redis");

            redisOptions.Configuration = connection;
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Any", corsPolicyBuilder =>
            {
                corsPolicyBuilder
                    .WithOrigins(new[] { "http://localhost:5173", "https://localhost:5173", "http://localhost:5174" })
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });


        var app = builder.Build();

        if (args.Length == 1 && args[0].ToLower() == "--seeddata")
            await SeedData(app);

        async Task SeedData(IHost app)
        {
            var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopedFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<Seed>();
                await service.SeedDataContextAsync();
            }
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        using (var scope = app.Services.CreateScope())
        {
            var messagesService = scope.ServiceProvider.GetRequiredService<IMessagesService>();
            var result = await messagesService.RemoveKeyAsync();
            if (result.IsSuccess)
            {
                Console.WriteLine("Cache key deleted successfully");
            }
            else
            {
                Console.WriteLine($"Failed to delete cache key: {result.Error}");
            }
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCors("Any");

        app.MapControllers();

        app.Run();
    }
}