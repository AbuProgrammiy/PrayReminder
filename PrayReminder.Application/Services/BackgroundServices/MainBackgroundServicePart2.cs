using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Views;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
    }
}
