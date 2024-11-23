using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using PrayReminder.Application.Services.QuoteServices;
using PrayReminder.Application.Services.UserServices;
using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Enums;
using PrayReminder.Domain.Entities.Models;
using PrayReminder.Domain.Entities.Views;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = PrayReminder.Domain.Entities.Models.User;

namespace PrayReminder.Application.Services.BackgroundServices
{
    public partial class MainBackgroundService : BackgroundService
    {
        private readonly TelegramBotClient _bot;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUserService _userService;
        private readonly IQuoteService _quoteService;
        public string[] regions = ["Toshkent","Andijon", "Buxoro", "Sirdaryo", "Samarqand", "Surxandaryo", "Namangan", "Navoiy", "Jizzax", "Qashqadaryo", "Farg'ona", "Xiva", "Qoraqalpog'iston"];

        public MainBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _bot = new TelegramBotClient("8189457186:AAEOs1V0QPIGdohCJ0kBuZDwXYuqbx1jarY", cancellationToken: new CancellationToken());
            _bot.OnMessage += OnMessage;

            _serviceScopeFactory = serviceScopeFactory;
            _userService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUserService>();
            _quoteService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IQuoteService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckPrayTime();
                }
                catch (Exception ex)
                {
                    await SendAlertToAdmin($"CheckPrayTimeda xatolik yuzberdi\n\n{ex.Message}\n\n{ex}\n");
                }

                await Task.Delay(59000, stoppingToken);
            }
        }

        public async Task RemindPrayTime(string prayName,Region region,string currentTime)
        {
            IEnumerable<User> users = await _userService.GetUsersByRegion(region);
            string[] encorageToPray = ["namozga shoshiling ish qochib ketmaydi", "“Albatta, namoz mo‘minlarga vaqtida farz qilingandir” (Niso surasi, 103-oyat)", "yashang ishni tashang, namoz vaqti bo'ldi","Bilmaydiganlar jim tursa, kelishmovchiliklar kamaygan bo'lar edi.\n<b>Abu Homid G'azzoliy</b>","Ilm - yod olingani emas, balki foyda berganidir.\n<b>Imomi Shofeiy roximahumullox</b>","Qafasda tug'iladigan qushlar uchishni jinoyat deb biladilar."];
            Random random= new Random();

            if (prayName == "Quyosh")
            {
                foreach (User user in users)
                {
                    try
                    {
                        await _bot.SendTextMessageAsync(user.ChatId, $"<b>{prayName}</b> chiqmoqda {currentTime} ⏰\n\nQuyosh chiqayotgan payt namoz o'qilmaydi ☀️", parseMode: ParseMode.Html);
                    }
                    catch { }
                }
            }
            else if (prayName == "Bomdod")
            {
                foreach (User user in users)
                {
                    try
                    {
                        await _bot.SendTextMessageAsync(user.ChatId, $"<b>{prayName}</b> namozi vaqti bo'ldi {currentTime} ⏰\n\nAllbatta namoz uyqudan afzaldir!", parseMode: ParseMode.Html);
                    }
                    catch { }
                }
            }
            else
            {
                foreach (User user in users)
                {
                    try
                    {
                        List<Quote> quotes = (List<Quote>) await _quoteService.GetAll();

                        int RndQuote=random.Next(0,quotes.Count);

                        await _bot.SendTextMessageAsync(user.ChatId, $"<b>{prayName}</b> namozi vaqti bo'ldi {currentTime} ⏰\n\n{quotes[RndQuote].Body}\n<b>{quotes[RndQuote].Author}</b>", parseMode: ParseMode.Html);
                    }
                    catch { }
                }
            }
        }

        public async Task IntroduceCommands(Message msg)
        {
            await _bot.SendTextMessageAsync(msg.Chat.Id, "start - Boshlash\r\n/region - Viloyatni tanlash\r\n/todaysprays - Bugungi kun namozlari\r\n/commands - Buyruqlar bilan tanishish\r\n/botinfo - Bot haqida ma'lumotlar", replyMarkup:new ReplyKeyboardRemove());
        }

        public async Task DefaultResponse(Message msg)
        {
            await _bot.SendTextMessageAsync(msg.Chat, "Tushunarsiz buyruq.\n /commands orqali buyruqlar bilan tanishishingiz mumkin!", replyMarkup: new ReplyKeyboardRemove());
        }

        public async Task SendBotInfo(Message msg)
        {
            int? usersCount = await _userService.GetAllUsersCount();
            await _bot.SendTextMessageAsync(msg.Chat.Id, $"Bot egasi @Abu_Programmiy 😁\n\nFoydalanuvchilar soni: {usersCount}\n\nFoydalanilgan ma'nbalar:\nislomapi.uz\naladhan.com");
        }

        public async Task SendMessageToEveryone(Message msg)
        {
            IEnumerable<User> users = await _userService.GetAll();

            msg.Text = msg.Text.Split(":admin")[0];

            foreach (User user in users)
            {
                try
                {
                    await _bot.SendTextMessageAsync(user.ChatId, msg.Text);
                }
                catch (Exception ex)
                {
                }
            }

            await _bot.SendTextMessageAsync(msg.Chat.Id, "Barchaga yuborildi ✅");
        }

        public async Task SendTodaysPrays(Message msg)
        {
            string messageToSend;

            DateTime dateTime=DateTime.Now;
            CultureInfo cultureInfo = new CultureInfo("uz-UZ");

            HttpClient client=new HttpClient();
            string dateInfo = await client.GetStringAsync("https://api.aladhan.com/v1/gToH/" + dateTime.ToString("dd-MM-yyyy"));

            string region=await _userService.GetUserRegionByChatId(msg.Chat.Id);

            string prayTimes = await client.GetStringAsync($"https://islomapi.uz/api/present/day?region={region}");

            JObject data=JObject.Parse(dateInfo);
            
            messageToSend = $"<b>Hijriy sana:</b> {data["data"]["hijri"]["date"]?.ToString()}\n<b>Melodiy sana:</b> {dateTime.ToString("dd-MM-yyyy")}\n\n<b>Hijriy oy</b>: {data["data"]["hijri"]["month"]["en"]} ({data["data"]["hijri"]["month"]["ar"]})\n<b>Melodiy oy:</b> {cultureInfo.DateTimeFormat.GetMonthName(dateTime.Month)}\n\n<b>Hijriy hafta kuni:</b> {data["data"]["hijri"]["weekday"]["en"]} ({data["data"]["hijri"]["weekday"]["ar"]})\n<b>Melodiy hafta kuni:</b> {cultureInfo.DateTimeFormat.GetDayName(dateTime.DayOfWeek)}";

            data = JObject.Parse(prayTimes);

            messageToSend += $"\n\n<b>Viloyat:</b> {region}\n\n" +
                             $"<b>Namoz vaqtlari:</b>\n\n" +
                             $"{data["times"]["tong_saharlik"]}  BOMDOD ⏰\n\n" +
                             $"{data["times"]["quyosh"]}  QUYOSH ⏰\n\n" +
                             $"{data["times"]["peshin"]}  PESHIN ⏰\n\n" +
                             $"{data["times"]["asr"]}  ASR ⏰\n\n" +
                             $"{data["times"]["shom_iftor"]}  SHOM ⏰\n\n" +
                             $"{data["times"]["hufton"]}  HUFTON ⏰";

            await _bot.SendTextMessageAsync(msg.Chat.Id, messageToSend,parseMode:ParseMode.Html);
        }

        public async Task SendAlertToAdmin(string alertText)
        {
            await _bot.SendTextMessageAsync(1268306946, $"{alertText}\nsana: {DateTime.Now}");
        }

        public async Task AddQuotes(Message msg)
        {
            QuoteDTO quoteDTO = new QuoteDTO
            {
                Body = msg.Text.Split("iqtibos: ")[1].Split("\n")[0],
                Author = msg.Text.Contains("avftor: ") ? msg.Text.Split("avftor: ")[1] : null
            };

            ResponseModel response = await _quoteService.Create(quoteDTO);

            if (response.StatusCode == 200)
            {
                await _bot.SendTextMessageAsync(msg.Chat, "Muvafaqqiyatli qo'shildi ✅");
            }
            else if (response.StatusCode == 400)
            {
                await _bot.SendTextMessageAsync(msg.Chat, "Ushbu iqtibos oldindan mavjud ⚠️");
            }
            else
            {
                await _bot.SendTextMessageAsync(msg.Chat, "Nmadur xato ketdi ⚠❌");
            }
        }
    }
}
