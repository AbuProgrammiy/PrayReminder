using Microsoft.Extensions.DependencyInjection;
using PrayReminder.Application.Services.UserServices;

namespace PrayReminder.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
