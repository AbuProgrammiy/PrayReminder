using Microsoft.Extensions.DependencyInjection;
using PrayReminder.Application.Services.BackgroundServices;
using PrayReminder.Application.Services.UserServices;

namespace PrayReminder.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            services.AddHostedService<MainBackgroundService>();
            services.AddTransient<IUserService, UserService>();

            return services;
        }
    }
}
