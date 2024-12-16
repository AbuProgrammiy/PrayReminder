using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrayReminder.Application.Abstractions;
using PrayReminder.Application.Services.QuoteServices;
using PrayReminder.Application.Services.UserServices;
using Telegram.Bot;


namespace PrayReminder.Application.Services.BackgroundServices
{
    public partial class MainBackgroundService : BackgroundService
    {
        private readonly TelegramBotClient _bot;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUserService _userService;
        private readonly IQuoteService _quoteService;
        private readonly IApplicationDbContext _applicationDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public string[] regions = ["Toshkent","Andijon", "Buxoro", "Sirdaryo", "Samarqand", "Surxandaryo", "Namangan", "Navoiy", "Jizzax", "Qashqadaryo", "Farg'ona", "Xiva", "Qoraqalpog'iston"];

        public MainBackgroundService(IServiceScopeFactory serviceScopeFactory, IWebHostEnvironment webHostEnvironment)
        {
            _bot = new TelegramBotClient("8189457186:AAEOs1V0QPIGdohCJ0kBuZDwXYuqbx1jarY", cancellationToken: new CancellationToken());
            _bot.OnMessage += OnMessage;

            _serviceScopeFactory = serviceScopeFactory;
            _userService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUserService>();
            _quoteService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IQuoteService>();
            _applicationDbContext = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IApplicationDbContext>();
            _webHostEnvironment = webHostEnvironment;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SavePrayTimes();
                    await CheckPrayTime();
                }
                catch (Exception ex)
                {
                    await SendAlertToAdmin($"CheckPrayTimeda xatolik yuzberdi\n\n{ex.Message}\n\n{ex}\n");
                }

                await Task.Delay(59990, stoppingToken);
            }
        }
    }
}
