using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Enums;
using PrayReminder.Domain.Entities.Models;
using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.Application.Services.UserServices
{
    public interface IUserService
    {
        public Task<IEnumerable<User>> GetAll();
        public Task<IEnumerable<User>> GetUsersByRegion(Region region);
        public Task<int> GetAllUsersCount();
        public Task<string> GetUserRegionByChatId(long chatId);
        public Task<ResponseModel> Create(CreateUserDTO request);
        public Task<ResponseModel> UpdateRegion(UpdateUserRegionDTO request);
        public Task<ResponseModel> DeleteUserById(Guid userId);
    }
}
