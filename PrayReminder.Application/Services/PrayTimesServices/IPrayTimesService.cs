using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.Application.Services.PrayTimesServices
{
    public interface IPrayTimesService
    {
        public Task<ResponseModel> GetDailyPrayTimes(string regionName, string date);
        public Task<ResponseModel> GetMonthlyPrayTimes(string regionName, string date);
    }
}
