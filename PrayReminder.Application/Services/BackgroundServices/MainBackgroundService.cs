using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using PrayReminder.Application.Services.UserServices;
using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Enums;
using PrayReminder.Domain.Entities.Views;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = PrayReminder.Domain.Entities.Models.User;

namespace PrayReminder.Application.Services.BackgroundServices
{
    public class MainBackgroundService : BackgroundService
    {
        private readonly TelegramBotClient _bot;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IUserService _userService;
        public string[] regions = ["Andijon", "Buxoro", "Farg'ona", "Jizzax", "Namangan", "Navoiy", "Samarqand", "Toshkent"];

        public MainBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _bot = new TelegramBotClient("8189457186:AAEOs1V0QPIGdohCJ0kBuZDwXYuqbx1jarY", cancellationToken: new CancellationToken());
            _bot.OnMessage += OnMessage;

            _serviceScopeFactory = serviceScopeFactory;
            _userService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IUserService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {

                CheckPrayTime();

                await Task.Delay(30000, stoppingToken);
            }
        }

        public async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.Text is null) return;

            if (msg.Text == "/start")
            {
                RegisterUser(msg);
            }
            else if (msg.Text == "/region")
            {
                ChooseRegion(msg);
            }
            else if (regions.Contains(msg.Text))
            {
                SelectRegion(msg);
            }
            else if (msg.Text == "/commands")
            {
                IntroduceCommands(msg);
            }
            else
            {
                DefaultResponse(msg);
            }
        }

        public async void RegisterUser(Message msg)
        {
            ResponseModel responseModel = await _userService.Create(new CreateUserDTO
            {
                ChatId = msg.Chat.Id,
                UserName = msg.Chat.Username,
                FirstName = msg.Chat.FirstName,
                LastName = msg.Chat.LastName
            });

            if (responseModel.StatusCode == 400)
            {
                await _bot.SendTextMessageAsync(msg.Chat, $"Hurmatli {msg.Chat.FirstName}aka sizni yana bir bor ko'rib turganimdan hursandman 🙂😊\nRegionni o'zgartirish uchun /region yuboring");
            }
            else if (responseModel.StatusCode == 200)
            {
                await _bot.SendTextMessageAsync(msg.Chat, "Assalamualeykum xush kelibsiz 🙂😊");
                ChooseRegion(msg);
            }
            else if (responseModel.StatusCode == 500)
            {
                await _bot.SendTextMessageAsync(msg.Chat, "Botimizda mummo yuz berdi, iltimos keyinroq aloqaga chiqing 🙂");
            }
        }

        public async void ChooseRegion(Message msg)
        {
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(true)
                                                .AddNewRow("Andijon", "Buxoro", "Farg'ona")
                                                .AddNewRow("Jizzax", "Namangan", "Navoiy")
                                                .AddNewRow("Samarqand", "Toshkent");

            await _bot.SendTextMessageAsync(msg.Chat,"Quyda Viloyatingizni tanlang:",replyMarkup: replyMarkup);
        }

        public async void SelectRegion(Message msg)
        {
            UpdateUserRegionDTO updateUserRegionDTO = new UpdateUserRegionDTO
            {
                ChatId = msg.Chat.Id
            };

            switch (msg.Text)
            {
                case "Andijon":
                    updateUserRegionDTO.Region = Region.Andijon;
                    break;
                case "Buxoro":
                    updateUserRegionDTO.Region = Region.Buxoro;
                    break;
                case "Farg'ona":
                    updateUserRegionDTO.Region = Region.Fargona;
                    break;
                case "Jizzax":
                    updateUserRegionDTO.Region = Region.Jizzax;
                    break;
                case "Namangan":
                    updateUserRegionDTO.Region = Region.Namangan;
                    break;
                case "Navoiy":
                    updateUserRegionDTO.Region = Region.Navoiy;
                    break;
                case "Samarqand":
                    updateUserRegionDTO.Region = Region.Samarqand;
                    break;
                case "Toshkent":
                    updateUserRegionDTO.Region = Region.Toshkent;
                    break;
            }

            ResponseModel response= await _userService.UpdateRegion(updateUserRegionDTO);

            if (response.IsSuccess)
            {
                await _bot.SendTextMessageAsync(msg.Chat.Id, "Viloyatingiz muvaffaqiyatli tanlandi.\nEndi har doim namoz vaqti kirganda sizga eslataman. 😊\n Agar viloyatingizni o'zgartirmoqchi bo'lsangiz /region yuboring.", replyMarkup: new ReplyKeyboardRemove());
            }
            else
            {
                await _bot.SendTextMessageAsync(msg.Chat.Id, "Nmadir xato ketdi, keyinroq urinib ko'ring!", replyMarkup: new ReplyKeyboardRemove());
            }
        }

        public async void CheckPrayTime()
        {
            string baseURL = "https://islomapi.uz/api/present/day?region=";
            HttpClient client = new HttpClient();
            DateTime currentDateTime = DateTime.Now;

            for (byte i = 0;i< regions.Count();i++)
            {
                string data = await client.GetStringAsync(baseURL + regions[i]);

                JObject prayTimes=JObject.Parse(data);
                string bomdod = prayTimes["times"]["tong_saharlik"].ToString();
                string quyosh = prayTimes["times"]["quyosh"].ToString();
                string peshin = prayTimes["times"]["peshin"].ToString();
                string asr = prayTimes["times"]["asr"].ToString();
                string shom = prayTimes["times"]["shom_iftor"].ToString();
                string hufton = prayTimes["times"]["hufton"].ToString();

                if (TimeOnly.TryParse(bomdod, out TimeOnly bomdodVaqti)==true&&bomdodVaqti==TimeOnly.FromDateTime(currentDateTime))
                {
                    RemindPrayTime("Bomdod", DefineRegion(regions[i]), bomdod);
                }
                else if(TimeOnly.TryParse(quyosh, out TimeOnly quyoshVaqti) == true && quyoshVaqti == TimeOnly.FromDateTime(currentDateTime))
                {
                    RemindPrayTime("Quyosh", DefineRegion(regions[i]), quyosh);
                }
                else if (TimeOnly.TryParse(peshin, out TimeOnly peshinVaqti) == true && peshinVaqti == TimeOnly.FromDateTime(currentDateTime))
                {
                    RemindPrayTime("Peshin", DefineRegion(regions[i]), peshin);
                }
                else if (TimeOnly.TryParse(asr, out TimeOnly asrVaqti) == true && asrVaqti == TimeOnly.FromDateTime(currentDateTime))
                {
                    RemindPrayTime("Asr", DefineRegion(regions[i]), asr);
                }
                else if (TimeOnly.TryParse(shom, out TimeOnly shomVaqti) == true && shomVaqti == TimeOnly.FromDateTime(currentDateTime))
                {
                    RemindPrayTime("Shom", DefineRegion(regions[i]), shom);
                }
                else if (TimeOnly.TryParse(hufton, out TimeOnly huftonVaqti) == true && huftonVaqti == TimeOnly.FromDateTime(currentDateTime))
                {
                    RemindPrayTime("Hufton", DefineRegion(regions[i]), hufton);
                }
            }

        }

        public Region DefineRegion(string regionName)
        {
            switch (regionName)
            {
                case "Andijon":
                    return Region.Andijon;
                case "Buxoro":
                    return Region.Buxoro;
                case "Farg'ona":
                    return Region.Fargona;
                case "Jizzax":
                    return Region.Jizzax;
                case "Namangan":
                    return Region.Namangan;
                case "Navoiy":
                    return Region.Navoiy;
                case "Samarqand":
                    return Region.Samarqand;
                case "Toshkent":
                    return Region.Toshkent;
                default:
                    throw new Exception();
            }
        }

        public async void RemindPrayTime(string prayName,Region region,string currentTime)
        {
            IEnumerable<User> users = await _userService.GetUsersByRegion(region);
            string[] encorageToPray = ["namozga shoshiling ish qochib ketmaydi", "“Albatta, namoz mo‘minlarga vaqtida farz qilingandir” (Niso surasi, 103-oyat)", "yashang ishni tashang, namoz vaqti bo'ldi"];
            Random random= new Random();

            if (prayName != "Quyosh")
            {
                foreach (User user in users)
                {
                    await _bot.SendTextMessageAsync(user.ChatId, $"<b>{prayName}</b> namozi vaqti bo'ldi {currentTime}⏰\n{encorageToPray[random.Next(0, encorageToPray.Count())]}", parseMode: ParseMode.Html);
                }
            }
            else
            {
                foreach (User user in users)
                {
                    await _bot.SendTextMessageAsync(user.ChatId, $"<b>{prayName}</b> chiqmoqda {currentTime}⏰\nQuyosh chiqayotgan payt namoz o'qilmaydi.", parseMode: ParseMode.Html);
                }
            }
        }

        public async void IntroduceCommands(Message msg)
        {
            await _bot.SendTextMessageAsync(msg.Chat.Id, "Bot uchun buyruqlar:\n/start - boshlash\n/region - viloyatni o'zgartirish\n/commands - buyruqlar bilan tanishish",replyMarkup:new ReplyKeyboardRemove());
        }

        public async void DefaultResponse(Message msg)
        {
            await _bot.SendTextMessageAsync(msg.Chat.Id, "Tushunarsiz buyruq.\n /commands orqali buyruqlar bilan tanishishingiz mumkin!", replyMarkup: new ReplyKeyboardRemove());
        }
    }
}
