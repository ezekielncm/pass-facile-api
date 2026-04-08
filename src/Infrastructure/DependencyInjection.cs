using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Services;
using Infrastructure.Auth;
using Infrastructure.Identity;
using Infrastructure.Persistences;
using Infrastructure.Persistences.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Messaging;
using Infrastructure.Services;
using Microsoft.Extensions.Http;
namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(o =>
                o.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            services.AddDbContext<IDbContext>(o =>
                o.UseNpgsql(config.GetConnectionString("IdentityConnection"),
                b => b.MigrationsAssembly(typeof(IDbContext).Assembly.FullName)));

            services.AddIdentity<AppUser, AppRole>(options =>
            {

                options.User.RequireUniqueEmail = false; // pas d'email !
                options.Password.RequiredLength = 0;     // pas de password
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<IDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
            services.Configure<IkkodiSettings>(config.GetSection("ExternalApis:Ikkodi"));
            //services.Configure<TwilioSettings>(config.GetSection("Twilio"));

            services.AddMemoryCache();
            services.AddScoped<JwtTokenGenerator>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IAuth, AuthService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUnitOfWork,UnitOfWork>();
            services.AddScoped<IEventPublisher,EventPublisher>();

            // Repositories
            services.AddScoped<IUserRepository, EfCoreUserRepository>();
            services.AddScoped<IEventRepository, EfCoreEventRepository>();
            services.AddScoped<IOrderRepository, EfCoreOrderRepository>();
            services.AddScoped<ITicketRepository, EfCoreTicketRepository>();
            services.AddScoped<IScanSessionRepository, EfCoreScanSessionRepository>();
            services.AddScoped<IOrganizerWalletRepository, EfCoreOrganizerWalletRepository>();
            services.AddScoped<INotificationRequestRepository, EfCoreNotificationRequestRepository>();

            services.AddHttpClient<IkkodiClient>();

            return services;
        }
    }
}
