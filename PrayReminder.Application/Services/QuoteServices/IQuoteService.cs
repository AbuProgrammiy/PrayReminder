using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Models;
using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.Application.Services.QuoteServices
{
    public interface IQuoteService
    {
        public Task<IEnumerable<Quote>> GetAll();
        public Task<ResponseModel> CreateRange(IEnumerable<CreateQuoteDTO> createQuoteDTOs);
        public Task<ResponseModel> Create(CreateQuoteDTO quoteDTO);
        public Task<ResponseModel> Delete(int id);
    }
}
