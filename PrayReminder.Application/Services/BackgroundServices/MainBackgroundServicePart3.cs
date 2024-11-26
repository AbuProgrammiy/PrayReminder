using Newtonsoft.Json.Linq;
using PrayReminder.Domain.Entities.Enums;
using PrayReminder.Domain.Entities.Models;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace PrayReminder.Application.Services.BackgroundServices
{
    public partial class MainBackgroundService
    {
        public async Task SavePrayTimes()
        {
            byte currentMonth = (byte)DateOnly.FromDateTime(DateTime.Now).Month;
            string baseUrl = "https://www.islom.uz/vaqtlar/";
            HttpClient client = new HttpClient();

            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
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

            List<PrayTimes> prayTimes = await _applicationDbContext.PrayTimes.Where(p => p.Date == DateOnly.FromDateTime(currentDateTime.Date)).ToListAsync();
            Console.WriteLine(prayTimes.Count);
            for (int i = 0; i < prayTimes.Count; i++)
            {
                Console.WriteLine($"{prayTimes[i].Bomdod} {prayTimes[i].Quyosh} {prayTimes[i].Peshin} {prayTimes[i].Asr} {prayTimes[i].Shom} {prayTimes[i].Xufton}");
            }
        }

        public async Task RemindPrayTime(string prayName, Region region, string currentTime)
        {
            
        }

        public async Task SendAlertToAdmin(string alertText)
        {
            await _bot.SendTextMessageAsync(1268306946, $"{alertText}\nsana: {DateTime.Now}");
        }
    }
}
