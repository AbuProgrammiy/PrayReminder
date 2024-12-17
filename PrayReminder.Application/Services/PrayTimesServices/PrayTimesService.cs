using Mapster;
using Microsoft.EntityFrameworkCore;
using PrayReminder.Application.Abstractions;
using PrayReminder.Domain.Entities.Enums;
using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.Application.Services.PrayTimesServices
{
    public class PrayTimesService : IPrayTimesService
    {
        private readonly IApplicationDbContext _applicationDbContext;

        public PrayTimesService(IApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<ResponseModel> GetDailyPrayTimes(string regionName, string date)
        {
            try
            {
                DateOnly dateOnly;

                try
                {
                    dateOnly = DateOnly.ParseExact(date, "yyyy-MM-dd");
                }
                catch
                {
                    return new ResponseModel
                    {
                        IsSuccess = true,
                        StatusCode = 400,
                        Response = "Date format should be: yyyy-MM-dd"
                    };
                }

                Region region = (Region)DefineRegion(regionName);

                PrayTimesView prayTimes = (await _applicationDbContext.PrayTimes.FirstOrDefaultAsync(p => p.Region == region && p.Date == dateOnly)).Adapt<PrayTimesView>(); ;

                return new ResponseModel
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Response = prayTimes
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Response = $"Something went wrong: {ex.Message}"
                };
            }
        }

        public async Task<ResponseModel> GetMonthlyPrayTimes(string regionName, string date)
        {
            try
            {
                DateOnly dateOnly;

                try
                {
                    dateOnly = DateOnly.ParseExact(date, "yyyy-MM");
                }
                catch
                {
                    return new ResponseModel
                    {
                        IsSuccess = true,
                        StatusCode = 400,
                        Response = "Date format should be: yyyy-MM"
                    };
                }

                Region region = (Region)DefineRegion(regionName);

                IEnumerable<PrayTimesView> prayTimes = (await _applicationDbContext.PrayTimes.Where(p => p.Region == region && p.Date.Year == dateOnly.Year && p.Date.Month == dateOnly.Month).ToListAsync()).Adapt<IEnumerable<PrayTimesView>>();

                return new ResponseModel
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Response = prayTimes
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    StatusCode = 500,
                    Response = $"Something went wrong: {ex.Message}"
                };
            }
        }

        private object DefineRegion(string regionName)
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
                    return -1;
            }
        }
    }
}
