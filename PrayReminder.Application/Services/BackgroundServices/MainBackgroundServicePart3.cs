using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PrayReminder.Domain.Entities.Enums;
using PrayReminder.Domain.Entities.Models;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PrayReminder.Application.Services.BackgroundServices
{
    public partial class MainBackgroundService
    {
        public async Task SavePrayTimes()
        {
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);

            PrayTimes pray = await _applicationDbContext.PrayTimes.FirstOrDefaultAsync(p => p.Date.Month == currentDate.Month&&p.Date.Year==currentDate.Year);

            if (pray != null)
            {
                return;
            }

            byte currentMonth = (byte)currentDate.Month;
            string baseUrl = "https://www.islom.uz/vaqtlar/";
            HttpClient client = new HttpClient();

            List<PrayTimes> prayTimesList = new List<PrayTimes>();

            foreach (Region region in Enum.GetValues(typeof(Region)))
            {
                string htmlData = await client.GetStringAsync($"{baseUrl}{(int)region}/{currentMonth}");

                string pattern = @">(\d{2}:\d{2})<";
                MatchCollection matches = Regex.Matches(htmlData, pattern);
                List<string> monthlyPrayTimes = matches.Cast<Match>().Select(m => m.Value.Replace(">","").Replace("<","")).ToList();

                byte day = 1;

                for (byte i = 0;i<monthlyPrayTimes.Count-3;i+=6)
                {
                    PrayTimes prayTimes = new PrayTimes
                    {
                        Bomdod = TimeOnly.Parse(monthlyPrayTimes[i]),
                        Quyosh = TimeOnly.Parse(monthlyPrayTimes[i + 1]),
                        Peshin = TimeOnly.Parse(monthlyPrayTimes[i + 2]),
                        Asr = TimeOnly.Parse(monthlyPrayTimes[i + 3]),
                        Shom = TimeOnly.Parse(monthlyPrayTimes[i + 4]),
                        Xufton = TimeOnly.Parse(monthlyPrayTimes[i + 5]),
                        Region= region,
                        Date=DateOnly.Parse($"{currentDate.Year}-{currentDate.Month}-{day}")
                    };
                    prayTimesList.Add(prayTimes);
                    day++;
                }
            }

            await _applicationDbContext.PrayTimes.AddRangeAsync(prayTimesList);
            await _applicationDbContext.SaveChangesAsync(new CancellationToken());

            client.Dispose();
        }

        public async Task CheckPrayTime()
        {
            DateTime currentDateTime= DateTime.Now;
            DateOnly currentDate=DateOnly.FromDateTime(currentDateTime);
            TimeOnly currentTime=TimeOnly.FromDateTime(currentDateTime);

            List<PrayTimes> prayTimes = await _applicationDbContext.PrayTimes.Where(p => p.Date == currentDate).ToListAsync();
            Console.WriteLine(prayTimes.Count);

            for (int i = 0; i < prayTimes.Count; i++)
            {
                Console.WriteLine($"{prayTimes[i].Bomdod} {prayTimes[i].Quyosh} {prayTimes[i].Peshin} {prayTimes[i].Asr} {prayTimes[i].Shom} {prayTimes[i].Xufton}");
                if (i == 11)
                {
                    Console.WriteLine($"{prayTimes[i].Bomdod.ToString("HH:mm")}=={currentTime.ToString("HH:mm")}");
                }
                if (prayTimes[i].Bomdod.ToString("HH:mm") == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Bomdod", prayTimes[i].Region, currentTime.ToString("HH:mm"));
                }
                else if(prayTimes[i].Quyosh.ToString("HH:mm") == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Quyosh", prayTimes[i].Region, currentTime.ToString("HH:mm"));
                }
                else if (prayTimes[i].Peshin.ToString("HH:mm") == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Peshin", prayTimes[i].Region, currentTime.ToString("HH:mm"));
                }
                else if (prayTimes[i].Asr.ToString("HH:mm") == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Asr", prayTimes[i].Region, currentTime.ToString("HH:mm"));
                }
                else if (prayTimes[i].Shom.ToString("HH:mm") == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Shom", prayTimes[i].Region, currentTime.ToString("HH:mm"));
                }
                else if (prayTimes[i].Xufton.ToString("HH:mm") == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Xufton", prayTimes[i].Region, currentTime.ToString("HH:mm"));
                }
            }
        }

        public async Task RemindPrayTime(string prayName, Region region, string currentTime)
        {
            List<User> users = await _applicationDbContext.Users.Where(u => u.Region == region&&u.IsBlocked==false).ToListAsync();
            JArray countsOfQuoteList = JArray.Parse(File.ReadAllText($"{_webHostEnvironment.WebRootPath}/SystemFiles/counts.json"));
            int count = 0;

            foreach (JObject countsOfQuote in countsOfQuoteList)
            {
                if (countsOfQuote["region"].ToString() == region.ToString())
                {
                    count = (int)countsOfQuote["count"];
                    countsOfQuote["count"] = count+1;
                    break;
                }
            }

            File.WriteAllText($"{_webHostEnvironment.WebRootPath}/SystemFiles/counts.json",countsOfQuoteList.ToString());

            List<Quote> quotes = await _applicationDbContext.Quotes.ToListAsync();
            Quote quote = quotes[count%quotes.Count];

            for (int i = 0; i < users.Count; i++)
            {
                try
                {
                    if (prayName == "Quyosh")
                    {
                        await _bot.SendTextMessageAsync(users[i].ChatId, $"<b>{prayName}</b> chiqmoqda. 🌅\n⏰ {currentTime}\nMintaqa: {region}\n\n{quote.Body}\n<b>{quote.Author}</b>", parseMode: ParseMode.Html);
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(users[i].ChatId, $"<b>{prayName}</b> namozi vaqti bo'ldi.\n⏰ {currentTime}\nMintaqa: {region}\n\n{quote.Body}\n<b>{quote.Author}</b>",parseMode:ParseMode.Html);
                    }
                }
                catch
                {
                    users[i].IsBlocked = true;
                }
            }

            await _applicationDbContext.SaveChangesAsync(new CancellationToken());
        }

        public async Task SendAlertToAdmin(string alertText)
        {
            await _bot.SendTextMessageAsync(1268306946, $"{alertText}\nsana: {DateTime.Now}");
        }
    }
}
