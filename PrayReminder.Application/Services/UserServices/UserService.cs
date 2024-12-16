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

        public async Task<ResponseModel> CreateRange(IEnumerable<User> users)
        {
            try
            {
                await _applicationDbContext.Users.AddRangeAsync(users);
                await _applicationDbContext.SaveChangesAsync(new CancellationToken());

                return new ResponseModel
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Response = "Users successfully added!"
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

        public async Task<ResponseModel> Create(CreateUserDTO request)
        {
            try
            {
                User user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.ChatId == request.ChatId);

                if (user != null)
                {
                    user.UserName = request.UserName != null ? request.UserName : user.UserName;
                    user.FirstName = request.FirstName != null ? request.FirstName : user.FirstName;
                    user.LastName = request.LastName != null ? request.LastName : user.LastName;
                    user.IsBlocked = false;

                    _applicationDbContext.Users.Update(user);
                    await _applicationDbContext.SaveChangesAsync(new CancellationToken());

                    return new ResponseModel
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Response="User already registered!"
                    };
                }

                user = request.Adapt<User>();
                user.IsBlocked = false;

                await _applicationDbContext.Users.AddAsync(user,cancellationToken: new CancellationToken());
                await _applicationDbContext.SaveChangesAsync(new CancellationToken());

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

        public async Task<ResponseModel> DeleteUserById(int id)
        {
            try
            {
                User? user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        StatusCode = 400,
                        Response = "User not found to delete!"
                    };
                }


                _applicationDbContext.Users.Remove(user);
                await _applicationDbContext.SaveChangesAsync(new CancellationToken());

                return new ResponseModel
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Response = "Deleted successfuly!"
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
                return await _applicationDbContext.Users.ToListAsync(new CancellationToken());
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message,ex);
            }
        }

        public async Task<int> GetAllUsersCount()
        {
            try
            {
                List<User> users= await _applicationDbContext.Users.ToListAsync(new CancellationToken());
                return users.Count;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<string> GetUserRegionByChatId(long chatId)
        {
            try
            {
                User user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);

                switch (user.Region)
                {
                    case Region.Andijon:
                        return "Andijon";
                    case Region.Buxoro:
                        return "Buxoro";
                    case Region.Fargona:
                        return "Farg'ona";
                    case Region.Jizzax:
                        return "Jizzax";
                    case Region.Namangan:
                        return "Namangan";
                    case Region.Navoiy:
                        return "Navoiy";
                    case Region.Samarqand:
                        return "Samarqand";
                    case Region.Toshkent:
                        return "Toshkent";
                    default:
                        throw new Exception();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<IEnumerable<User>> GetUsersByRegion(Region region)
        {
            try
            {
                return await _applicationDbContext.Users.Where(u=>u.Region==region).ToListAsync(new CancellationToken());
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
                User? user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.ChatId == request.ChatId,cancellationToken: new CancellationToken());

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

                await _applicationDbContext.SaveChangesAsync(new CancellationToken());

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

        public async Task<ResponseModel> Update(User userDTO)
        {
            try
            {
                User user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.Id == userDTO.Id);

                if (user == null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        StatusCode = 404,
                        Response = $"User not found to update!"
                    };
                }

                user.FirstName = userDTO.FirstName;
                user.LastName = userDTO.LastName;
                user.UserName = userDTO.UserName;
                user.ChatId = userDTO.ChatId;
                user.Region = userDTO.Region;
                user.IsBlocked = userDTO.IsBlocked;

                await _applicationDbContext.SaveChangesAsync(new CancellationToken());

                return new ResponseModel
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Response = "User successfully updated!"
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
