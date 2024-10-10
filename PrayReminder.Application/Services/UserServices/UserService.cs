using Mapster;
using Microsoft.EntityFrameworkCore;
using PrayReminder.Application.Abstractions;
using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Enums;
using PrayReminder.Domain.Entities.Models;
using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.Application.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly IApplicationDbContext _applicationDbContext;

        public UserService(IApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<ResponseModel> Create(CreateUserDTO request)
        {
            try
            {
                if (await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.ChatId == request.ChatId) != null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Response="User already registered!"
                    };
                }

                User user = request.Adapt<User>();

                await _applicationDbContext.Users.AddAsync(user);
                await _applicationDbContext.SaveChangesAsync();

                return new ResponseModel
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Response = "User created successfuly!"
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

        public async Task<IEnumerable<User>> GetAll()
        {
            try
            {
                return await _applicationDbContext.Users.ToListAsync();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message,ex);
            }
        }

        public async Task<IEnumerable<User>> GetUsersByRegion(Region region)
        {
            try
            {
                return await _applicationDbContext.Users.Where(u=>u.Region==region).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<ResponseModel> UpdateRegion(UpdateUserRegionDTO request)
        {
            try
            {
                User user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.ChatId == request.ChatId);

                if (user == null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Response = "User not found to update region!"
                    };
                }

                user.Region= request.Region;

                await _applicationDbContext.SaveChangesAsync();

                return new ResponseModel
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Response = "Updated successfuly!"
                };
            }
            catch(Exception ex)
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
