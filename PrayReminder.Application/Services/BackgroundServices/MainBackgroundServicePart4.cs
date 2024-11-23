using PrayReminder.Domain.Entities.Enums;

namespace PrayReminder.Application.Services.BackgroundServices
{
    public partial class MainBackgroundService
    {
        public Region DefineRegion(string regionName)
        {
            switch (regionName)
            {
                case "Toshkent":
                    return Region.Toshkent;
                case "Andijon":
                    return Region.Andijon;
                case "Buxoro":
                    return Region.Buxoro;
                case "Sirdaryo":
                    return Region.Sirdaryo;
                case "Samarqand":
                    return Region.Samarqand;
                case "Surxandaryo":
                    return Region.Surxandaryo;
                case "Namangan":
                    return Region.Namangan;
                case "Navoiy":
                    return Region.Navoiy;
                case "Jizzax":
                    return Region.Jizzax;
                case "Qashqadaryo":
                    return Region.Qashqadaryo;
                case "Farg'ona":
                    return Region.Fargona;
                case "Xiva":
                    return Region.Xiva;
                case "Qoraqalpog'iston":
                    return Region.Qoraqalpogiston;
                default:
                    throw new Exception();
            }
        }
    }
}
