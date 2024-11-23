using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace PrayReminder.Application.Services.BackgroundServices
{
    public partial class MainBackgroundService
    {
        public async Task ManageCommands(Message msg)
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
    }
}
