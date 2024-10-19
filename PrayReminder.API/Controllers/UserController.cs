using Microsoft.AspNetCore.Mvc;
using PrayReminder.Application.Services.UserServices;
using PrayReminder.Domain.Entities.DTOs;
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

        [HttpDelete]
        public async Task<ResponseModel> DeleteUserById(Guid userId)
        {
            return await _userService.DeleteUserById(userId);
        }
    }
}
