using Mapster;
using Microsoft.EntityFrameworkCore;
using PrayReminder.Application.Abstractions;
using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Models;
using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.Application.Services.QuoteServices
{
    public class QuoteService : IQuoteService
    {
        private readonly IApplicationDbContext _applicationDbContext;

        public QuoteService(IApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<ResponseModel> Create(QuoteDTO quoteDTO)
        {
            try
            {
                Quote quote = await _applicationDbContext.Quotes.FirstOrDefaultAsync(q => q.Body == quoteDTO.Body);

                if (quote != null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Response = "Bunday iqtibos oldindan mavjud!"
                    };
                }

                quote = quoteDTO.Adapt<Quote>();

                await _applicationDbContext.Quotes.AddAsync(quote);
                await _applicationDbContext.SaveChangesAsync(new CancellationToken());

                return new ResponseModel
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Response = "Quote succesfully created!"
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

        public async Task<IEnumerable<Quote>> GetAll()
        {
            try
            {
                return await _applicationDbContext.Quotes.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message,ex);
            }
        }

        public async Task<ResponseModel> Delete(int id)
        {
            try
            {
                Quote quote=await _applicationDbContext.Quotes.FirstOrDefaultAsync(q=>q.Id == id);

                if (quote == null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Response = "Quote not found!"
                    };
                }

                _applicationDbContext.Quotes.Remove(quote);
                await _applicationDbContext.SaveChangesAsync(new CancellationToken());

                return new ResponseModel
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Response = "Quote successfully deleted!"
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
    }
}
