using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;

        public AccountController(DataContext context,
            ITokenService tokenService,
            IMapper mapper)
        {
            this.context = context;
            this.tokenService = tokenService;
            this.mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(await UserExists(registerDto.Username))
            {
                return BadRequest("Ysername is taken");
            }

            //vise mi ova funkcija ne triba jer mapiram direktno
            //AppUser UserForInsert = MapRegisterDTO(registerDto);
            AppUser UserForInsert = mapper.Map<AppUser>(registerDto);
            using var hmac = new HMACSHA512();
            UserForInsert.UserName = registerDto.Username.ToLower();
            UserForInsert.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            UserForInsert.PasswordSalt = hmac.Key;

            context.Users.Add(UserForInsert);
            await context.SaveChangesAsync();

            return new UserDto
            {
                Username = UserForInsert.UserName,
                Token = tokenService.CreateToken(UserForInsert),
                KnownAs = UserForInsert.KnownAs,
                Gender = UserForInsert.Gender
            };  
            
            
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.UserName);

            if(user == null)
            {
                return Unauthorized("Invalid username");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i = 0; i < user.PasswordHash.Length; i++)
            {
                if(user.PasswordHash[i] != computeHash[i])
                {
                    return Unauthorized("Invalid password");
                }
            }
            return new UserDto
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(f => f.IsMain==true)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        private AppUser MapRegisterDTO(RegisterDto registerDto)
        {
            using var hmac = new HMACSHA512();
            return new AppUser
            {
                UserName = registerDto.Username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}