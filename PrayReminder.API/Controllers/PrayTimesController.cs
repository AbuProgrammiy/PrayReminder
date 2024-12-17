using Microsoft.AspNetCore.Mvc;
using PrayReminder.Application.Services.PrayTimesServices;
using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PrayTimesController : ControllerBase
    {
        private readonly IPrayTimesService _rayTimesService;

        public PrayTimesController(IPrayTimesService rayTimesService)
        {
            _rayTimesService = rayTimesService;
        }

        [HttpGet]
        [Route("{regionName}/{date}")]
        public async Task<ResponseModel> GetDailyPrayTimes(string regionName, string date)
        {
            return await _rayTimesService.GetDailyPrayTimes(regionName, date);
        }

        [HttpGet]
        [Route("{regionName}/{date}")]
        public async Task<ResponseModel> GetMonthlyPrayTimes(string regionName, string date)
        {
            return await _rayTimesService.GetMonthlyPrayTimes(regionName, date);
        }
    }
}
