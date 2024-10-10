﻿using PrayReminder.Domain.Entities.DTOs;
using PrayReminder.Domain.Entities.Enums;
using PrayReminder.Domain.Entities.Models;
using PrayReminder.Domain.Entities.Views;

namespace PrayReminder.Application.Services.UserServices
{
    public interface IUserService
    {
        public Task<IEnumerable<User>> GetAll();
        public Task<IEnumerable<User>> GetUsersByRegion(Region region);
        public Task<ResponseModel> Create(CreateUserDTO request);
        public Task<ResponseModel> UpdateRegion(UpdateUserRegionDTO request);
    }
}
