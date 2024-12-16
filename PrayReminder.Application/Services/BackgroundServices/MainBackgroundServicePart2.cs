using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PrayReminder.Domain.Entities.DTOs;
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
                await SendAlertToAdmin($"{ex.Message}\n\n{ex}\n");
            }
        }

        public async Task RegisterUser(Message msg)
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
                await _bot.SendTextMessageAsync(msg.Chat.Id, "Mintaqangiz muvaffaqiyatli tanlandi.\nEndi har doim namoz vaqti kirganda sizga eslataman. 😊\nAgar viloyatingizni o'zgartirmoqchi bo'lsangiz /region yuboring.", replyMarkup: new ReplyKeyboardRemove());
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
            int activeUsersCount = (await _applicationDbContext.Users.Where(u => u.IsBlocked == false).ToListAsync()).Count;
            int blockedUsersCount = (await _applicationDbContext.Users.Where(u => u.IsBlocked == true).ToListAsync()).Count;
            await _bot.SendTextMessageAsync(msg.Chat.Id, $"Bot egasi @Abu_Programmiy 😁\nKanalga obuna bo'lish hozircha tekin: @AbuProgrammiy\n\nAktiv foydalanuvchilar soni: {activeUsersCount}\nBotni bloklagan foydalanuvchilar: {blockedUsersCount}\n\nFoydalanilgan ma'nbalar:\nislom.uz\nislomapi.uz\naladhan.com");
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

            DateOnly currentDate =DateOnly.FromDateTime(DateTime.Now);
            CultureInfo cultureInfo = new CultureInfo("uz-UZ");

            HttpClient client = new HttpClient();
            string dateInfo = await client.GetStringAsync("https://api.aladhan.com/v1/gToH/" + currentDate.ToString("dd-MM-yyyy"));

            JObject data = JObject.Parse(dateInfo);

            messageToSend = $"<b>Hijriy sana:</b> {data["data"]["hijri"]["date"]?.ToString()}\n<b>Melodiy sana:</b> {currentDate.ToString("dd-MM-yyyy")}\n\n<b>Hijriy oy</b>: {data["data"]["hijri"]["month"]["en"]} ({data["data"]["hijri"]["month"]["ar"]})\n<b>Melodiy oy:</b> {cultureInfo.DateTimeFormat.GetMonthName(currentDate.Month)}\n\n<b>Hijriy hafta kuni:</b> {data["data"]["hijri"]["weekday"]["en"]} ({data["data"]["hijri"]["weekday"]["ar"]})\n<b>Melodiy hafta kuni:</b> {cultureInfo.DateTimeFormat.GetDayName(currentDate.DayOfWeek)}";

            User user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.ChatId == msg.Chat.Id);

            PrayTimes prayTimes=await _applicationDbContext.PrayTimes.FirstOrDefaultAsync(p=>p.Date==currentDate&&p.Region==user.Region);

            messageToSend += $"\n\n" +
                             $"<b>Namoz vaqtlari:</b>\n\n" +
                             $"⏰ {prayTimes.Bomdod.ToString("HH:mm")}  <b>BOMDOD</b>\n\n" +
                             $"⏰ {prayTimes.Quyosh.ToString("HH:mm")}  <b>QUYOSH</b>\n\n" +
                             $"⏰ {prayTimes.Peshin.ToString("HH:mm")}  <b>PESHIN</b>\n\n" +
                             $"⏰ {prayTimes.Asr.ToString("HH:mm")}  <b>ASR</b>\n\n" +
                             $"⏰ {prayTimes.Shom.ToString("HH:mm")}  <b>SHOM</b>\n\n" +
                             $"⏰ {prayTimes.Xufton.ToString("HH:mm")}  <b>HUFTON</b>";

            messageToSend += $"\n\nMintaqa: <b>{user.Region}</b>";

            await _bot.SendTextMessageAsync(msg.Chat.Id, messageToSend, parseMode: ParseMode.Html);
        }



        public async Task AddQuotes(Message msg)
        {
            CreateQuoteDTO quoteDTO = new CreateQuoteDTO
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
