using Newtonsoft.Json.Linq;

namespace PrayReminder.Application.Services.BackgroundServices
{
    public partial class MainBackgroundService
    {
        public async Task CheckPrayTime()
        {
            string baseURL = "https://islomapi.uz/api/present/day?region=";
            HttpClient client = new HttpClient();
            DateTime currentDateTime = DateTime.Now;
            TimeOnly currentTime = TimeOnly.FromDateTime(currentDateTime);

            for (byte i = 0; i < regions.Count(); i++)
            {
                string data = await client.GetStringAsync(baseURL + regions[i]);

                JObject prayTimes = JObject.Parse(data);
                string bomdod = prayTimes["times"]["tong_saharlik"].ToString();
                string quyosh = prayTimes["times"]["quyosh"].ToString();
                string peshin = prayTimes["times"]["peshin"].ToString();
                string asr = prayTimes["times"]["asr"].ToString();
                string shom = prayTimes["times"]["shom_iftor"].ToString();
                string hufton = prayTimes["times"]["hufton"].ToString();

                if (bomdod == currentTime.ToString("HH:mm"))
                {
                    await RemindPrayTime("Bomdod", DefineRegion(regions[i]), bomdod);
                }
                else if (quyosh == currentTime.ToString("HH:mm"))
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
    }
}
