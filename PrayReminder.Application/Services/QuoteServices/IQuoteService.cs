using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Models;
using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.Application.Services.QuoteServices
{
    public interface IQuoteService
    {
        public Task<IEnumerable<Quote>> GetAll();
        public Task<ResponseModel> Create(QuoteDTO quoteDTO);
        public Task<ResponseModel> Delete(Guid id);
    }
}
