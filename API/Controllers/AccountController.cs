using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    //public enum dani
    //{
    //    pon,
    //    uto,
    //    sri
    //};
    //public interface idani
    //{
    //    public enum dani;
    //}
    public class AccountController : BaseApiController
    {
        //string text = dani.pon.Format();
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IMapper mapper)
        {
            
            this.userManager = userManager;
            this.signInManager = signInManager;
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
            //using var hmac = new HMACSHA512();
            UserForInsert.UserName = registerDto.Username.ToLower();
            //UserForInsert.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            //UserForInsert.PasswordSalt = hmac.Key;

            var result = await userManager.CreateAsync(UserForInsert, registerDto.Password);
            

            //context.Users.Add(UserForInsert);
            //await context.SaveChangesAsync();
            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = userManager.AddToRoleAsync(UserForInsert, "Member");
            if (!roleResult.IsCompletedSuccessfully) return BadRequest(result.Errors);

            return new UserDto
            {
                Username = UserForInsert.UserName,
                Token = await tokenService.CreateToken(UserForInsert, userManager),
                KnownAs = UserForInsert.KnownAs,
                Gender = UserForInsert.Gender
            };  
            
            
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.UserName.ToLower());

            if(user == null)
            {
                return Unauthorized("Invalid username");
            }

            //using var hmac = new HMACSHA512(user.PasswordSalt);

            //var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            //for(int i = 0; i < user.PasswordHash.Length; i++)
            //{
            //    if(user.PasswordHash[i] != computeHash[i])
            //    {
            //        return Unauthorized("Invalid password");
            //    }
            //}


            //var result = await signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

            //if(loginDto.UserName.ToLower() == "admin")
            //{
            //    user.UserRoles.Add(new AppUserRole()
            //    {
            //        Role = new AppRole() { }
            //    })
            //}
            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized();
            return new UserDto
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user, userManager),
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
                //PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                //PasswordSalt = hmac.Key
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}