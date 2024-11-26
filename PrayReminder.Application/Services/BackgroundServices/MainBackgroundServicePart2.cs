using Newtonsoft.Json.Linq;
using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Views;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = PrayReminder.Domain.Entities.Models.User;

namespace PrayReminder.Application.Services.BackgroundServices
{
    public partial class MainBackgroundService
    {
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
                else if (msg.Text == "/botinfo")
                {
                    await SendBotInfo(msg);
                }
                else if (msg.Text.Contains(":admin") && msg.Chat.Id == 1268306946)
                {
                    await SendMessageToEveryone(msg);
                }
                else if (msg.Text.Contains("iqtibos: "))
                {
                    await AddQuotes(msg);
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
            catch (Exception ex)
            {
                await SendAlertToAdmin(ex.Message + ex.InnerException);
            }
        }

        public async Task ChooseRegion(Message msg)
        {
            ReplyKeyboardMarkup replyMarkup = new ReplyKeyboardMarkup(true).AddNewRow("Toshkent", "Andijon", "Buxoro")
                                                                                        .AddNewRow("Sirdaryo", "Samarqand", "Surxandaryo")
                                                                                        .AddNewRow("Namangan", "Navoiy", "Jizzax")
                                                                                        .AddNewRow("Qashqadaryo", "Farg'ona", "Xiva")
                                                                                        .AddNewRow("Qoraqalpog'iston");

            await _bot.SendTextMessageAsync(msg.Chat, "Quyida mintaqangizni tanlang:", replyMarkup: replyMarkup);
        }

        public async Task SelectRegion(Message msg)
        {
            UpdateUserRegionDTO updateUserRegionDTO = new UpdateUserRegionDTO
            {
                ChatId = msg.Chat.Id
            };

            updateUserRegionDTO.Region = DefineRegion(msg.Text!);

            ResponseModel response = await _userService.UpdateRegion(updateUserRegionDTO);

            if (response.IsSuccess)
            {
                await _bot.SendTextMessageAsync(msg.Chat.Id, "Viloyatingiz muvaffaqiyatli tanlandi.\nEndi har doim namoz vaqti kirganda sizga eslataman. 😊\n Agar viloyatingizni o'zgartirmoqchi bo'lsangiz /region yuboring.", replyMarkup: new ReplyKeyboardRemove());
            }
            else
            {
                await _bot.SendTextMessageAsync(msg.Chat.Id, "Nmadir xato ketdi, keyinroq urinib ko'ring!", replyMarkup: new ReplyKeyboardRemove());
            }
        }

        public async Task IntroduceCommands(Message msg)
        {
            await _bot.SendTextMessageAsync(msg.Chat.Id, "start - Boshlash\r\n/region - Viloyatni tanlash\r\n/todaysprays - Bugungi kun namozlari\r\n/commands - Buyruqlar bilan tanishish\r\n/botinfo - Bot haqida ma'lumotlar", replyMarkup: new ReplyKeyboardRemove());
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

            DateTime dateTime = DateTime.Now;
            CultureInfo cultureInfo = new CultureInfo("uz-UZ");

            HttpClient client = new HttpClient();
            string dateInfo = await client.GetStringAsync("https://api.aladhan.com/v1/gToH/" + dateTime.ToString("dd-MM-yyyy"));

            string region = await _userService.GetUserRegionByChatId(msg.Chat.Id);

            string prayTimes = await client.GetStringAsync($"https://islomapi.uz/api/present/day?region={region}");

            JObject data = JObject.Parse(dateInfo);

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

            await _bot.SendTextMessageAsync(msg.Chat.Id, messageToSend, parseMode: ParseMode.Html);
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
