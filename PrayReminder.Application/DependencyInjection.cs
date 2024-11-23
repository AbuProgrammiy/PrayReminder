using Microsoft.Extensions.DependencyInjection;
using PrayReminder.Application.Services.BackgroundServices;
using PrayReminder.Application.Services.QuoteServices;
using PrayReminder.Application.Services.UserServices;

namespace PrayReminder.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            services.AddHostedService<MainBackgroundServicePart1>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IQuoteService, QuoteService>();

            return services;
        }
    }
}
