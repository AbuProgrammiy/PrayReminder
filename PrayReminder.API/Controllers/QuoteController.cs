using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrayReminder.Application.Services.QuoteServices;
using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Models;
using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class QuoteController : ControllerBase
    {
        private readonly IQuoteService _quoteService;

        public QuoteController(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        [HttpGet]
        public Task<IEnumerable<Quote>> GetAll()
        {
            return _quoteService.GetAll();
        }

        [HttpPost]
        public Task<ResponseModel> Create(QuoteDTO quoteDTO)
        {
            return _quoteService.Create(quoteDTO);
        }

        [HttpDelete]
        [Route("{id}")]
        public Task<ResponseModel> Delete(Guid id)
        {
            return _quoteService.Delete(id);
        }
    }
}
