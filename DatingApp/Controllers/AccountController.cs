﻿using DatingApp.Data;
using DatingApp.DTO;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext dataContext, ITokenService tokenService)
        {
            _dataContext = dataContext;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.UserName))
            {
                return BadRequest("Username is already taken");
            }

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = registerDto.UserName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();

            return user;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.UserName);

            if (user == null)
            {
                return Unauthorized("Invalid username");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i = 0; i < computedHash.Length; i++)
            {
                if(computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid password");
                }
            }

            return new UserDto
            {
                UserName = loginDto.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _dataContext.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
