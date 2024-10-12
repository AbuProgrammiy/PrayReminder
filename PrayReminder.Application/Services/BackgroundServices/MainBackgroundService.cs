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

        public async Task OnMessage(Message msg, UpdateType type)
        {
            try
            {
                if (msg.Text is null) return;

                if (msg.Text == "/start")
                {
                    await RegisterUser(msg);
                }
                else if (msg.Text == "/region")
                {
                    await ChooseRegion(msg);
                }
                else if (regions.Contains(msg.Text))
                {
                    await SelectRegion(msg);
                }
                else if (msg.Text == "/commands")
                {
                    await IntroduceCommands(msg);
                }
                else if (msg.Text == "/todaysprays")
                {
                    await SendTodaysPrays(msg);
                }
                else if (msg.Text=="/botinfo")
                {
                    await SendBotInfo(msg);
                }
                else if (msg.Text.Contains(":admin"))
                {
                    await SendMessageToEveryone(msg);
                }
                else
                {
                    await DefaultResponse(msg);
                }
            }
            catch (Exception ex)
            {
                await SendAlertToAdmin($"OnMessageda xatolik yuzberdi\n\n{ex.Message}\n\n{ex}\n");
            }
        }

        public async Task RegisterUser(Message msg)
        {
            try
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
                    await ChooseRegion(msg);
                }
                else if (responseModel.StatusCode == 500)
                {
                    await _bot.SendTextMessageAsync(msg.Chat, "Botimizda mummo yuz berdi, iltimos keyinroq aloqaga chiqing 🙂");
                }
            }
            catch
            {
                await _bot.SendTextMessageAsync(msg.Chat, "Nmadir xato ketdi(\nqayta urinig!");
            }
        }

        public async Task ChooseRegion(Message msg)
        {
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(true)
                                                .AddNewRow("Andijon", "Buxoro", "Farg'ona")
                                                .AddNewRow("Jizzax", "Namangan", "Navoiy")
                                                .AddNewRow("Samarqand", "Toshkent");

            await _bot.SendTextMessageAsync(msg.Chat,"Quyda Viloyatingizni tanlang:",replyMarkup: replyMarkup);
        }

        public async Task SelectRegion(Message msg)
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

        public async Task CheckPrayTime()
        {
            string baseURL = "https://islomapi.uz/api/present/day?region=";
            HttpClient client = new HttpClient();
            DateTime currentDateTime = DateTime.Now;
            TimeOnly currentTime=TimeOnly.FromDateTime(currentDateTime);

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

                if (bomdod==currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Bomdod", DefineRegion(regions[i]), bomdod);
                }
                else if(quyosh == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Quyosh", DefineRegion(regions[i]), quyosh);
                }
                else if (peshin == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Peshin", DefineRegion(regions[i]), peshin);
                }
                else if (asr == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Asr", DefineRegion(regions[i]), asr);
                }
                else if (shom == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Shom", DefineRegion(regions[i]), shom);
                }
                else if (hufton == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Hufton", DefineRegion(regions[i]), hufton);
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

        public async Task RemindPrayTime(string prayName,Region region,string currentTime)
        {
            IEnumerable<User> users = await _userService.GetUsersByRegion(region);
            string[] encorageToPray = ["namozga shoshiling ish qochib ketmaydi", "“Albatta, namoz mo‘minlarga vaqtida farz qilingandir” (Niso surasi, 103-oyat)", "yashang ishni tashang, namoz vaqti bo'ldi"];
            Random random= new Random();

            if (prayName != "Quyosh")
            {
                foreach (User user in users)
                {
                    try
                    {
                        await _bot.SendTextMessageAsync(user.ChatId, $"<b>{prayName}</b> namozi vaqti bo'ldi {currentTime} ⏰\n\n{encorageToPray[random.Next(0, encorageToPray.Count())]}", parseMode: ParseMode.Html);
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
                        await _bot.SendTextMessageAsync(user.ChatId, $"<b>{prayName}</b> chiqmoqda {currentTime} ⏰\n\nQuyosh chiqayotgan payt namoz o'qilmaydi ☀️", parseMode: ParseMode.Html);
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
            int usersCount=await _userService.GetAllUsersCount();
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

            HttpClient client=new HttpClient();
            string dateInfo = await client.GetStringAsync("https://api.aladhan.com/v1/gToH/" + DateTime.Now.ToString("dd-MM-yyyy"));

            string region=await _userService.GetUserRegionByChatId(msg.Chat.Id);

            string prayTimes = await client.GetStringAsync($"https://islomapi.uz/api/present/day?region={region}");


            JObject data=JObject.Parse(dateInfo);
            
            messageToSend = $"<b>Hijriy sana:</b> {data["data"]["hijri"]["date"]?.ToString()}\n\n<b>Oy</b>: {data["data"]["hijri"]["month"]["en"]} ({data["data"]["hijri"]["month"]["ar"]})\n<b>Hafta kuni:</b> {data["data"]["hijri"]["weekday"]["en"]} ({data["data"]["hijri"]["weekday"]["ar"]})";

            data = JObject.Parse(prayTimes);

            messageToSend += $"\n\n<b>Viloyat:</b> {region}\n\n<b>Namoz vaqtlari:</b>\nBomdod: {data["times"]["tong_saharlik"]} ⏰\nQuyosh: {data["times"]["quyosh"]} ⏰\nPeshin: {data["times"]["peshin"]} ⏰\nAsr: {data["times"]["asr"]} ⏰\nShom: {data["times"]["shom_iftor"]} ⏰\nHufton: {data["times"]["hufton"]} ⏰";

            await _bot.SendTextMessageAsync(msg.Chat.Id, messageToSend,parseMode:ParseMode.Html);
        }

        public async Task SendAlertToAdmin(string alertText)
        {
            await _bot.SendTextMessageAsync(1268306946, $"{alertText}\nsana: {DateTime.Now}");
        }
    }
}
