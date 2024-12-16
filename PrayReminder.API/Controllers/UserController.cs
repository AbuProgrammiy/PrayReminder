using Microsoft.AspNetCore.Mvc;
using PrayReminder.Application.Services.UserServices;
using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Enums;
using PrayReminder.Domain.Entities.Models;
using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IEnumerable<User>> GetAll()
        {
            return await _userService.GetAll();
        }

        [HttpGet]
        [Route("{region}")]
        public async Task<IEnumerable<User>> GetUsersByRegion(Region region)
        {
            return await _userService.GetUsersByRegion(region);
        }

        [HttpGet]
        public async Task<int> GetAllUsersCount()
        {
            return await _userService.GetAllUsersCount();
        }

        [HttpPost]
        public async Task<ResponseModel> Create(CreateUserDTO user)
        {
            return await _userService.Create(user);
        }

        [HttpPost]
        public async Task<ResponseModel> CreateRange(IEnumerable<User> users)
        {
            return await _userService.CreateRange(users);
        }

        [HttpPut]
        public async Task<ResponseModel> Update(User users)
        {
            return await _userService.Update(users);
        }

        [HttpDelete]
        public async Task<ResponseModel> DeleteUserById(int id)
        {
            return await _userService.DeleteUserById(id);
        }
    }
}
