using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public UsersController(IUserRepository userRepository,
            IMapper mapper,
            IPhotoService photoService)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
        {
            var users = await userRepository.GetMembersAsync();

            //var usersToReturn = mapper.Map<IEnumerable<MemberDTO>>(users);

            return Ok(users);
        }

        [HttpGet("{username}", Name="FetchUser")]
        public async Task<ActionResult<MemberDTO>> GetUser(string username)
        {
            var user = await userRepository.GetMemberAsync(username);
            
            //var usersoReturn = mapper.Map<MemberDTO>(user);

            return Ok(user);

        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await userRepository.GetUserByUsernameAsync(username);

            mapper.Map(memberUpdateDto, user);

            userRepository.Update(user);

            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> UploadPhoto(IFormFile file)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await userRepository.GetUserByUsernameAsync(username);
            
            
            var uploadResult = await photoService.AddPhotoAsync(file);

            if (uploadResult.Error != null)
            {
                return BadRequest(uploadResult.Error.Message);
            }
            var photo = new Photo()
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl.ToString()
            };
            
            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
            //4 api saves photoUrl and public ID to db
            user.Photos.Add(photo); //dodali pravi photo
            //clientu treba samo photoDTO najs
            if(await userRepository.SaveAllAsync())
            {
                return CreatedAtRoute("FetchUser",
                    new { username = user.UserName },
                    mapper.Map<PhotoDTO>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await userRepository.GetUserByUsernameAsync(username);

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            var currentMain = user.Photos.FirstOrDefault(f => f.IsMain == true);
            currentMain.IsMain = false;
            //foreach(var f in user.Photos)
            //{
            //    f.IsMain = false;
            //}
            photo.IsMain = true;

            if (await userRepository.SaveAllAsync())
            {
                return Ok("nesto nesto");//NoContent();
            }
            return BadRequest("Problem adding main photo");

        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await userRepository.GetUserByUsernameAsync(username);

            if (user == null)
                return BadRequest("No username existing");

            var photoToDelete = user.Photos.FirstOrDefault(f => f.Id == photoId);
            if (photoToDelete == null)
                return NotFound();//BadRequest("Cannot find image");
            if (photoToDelete.IsMain == true)
                return BadRequest("Cannot delete main photo");

            //delete photo from cloudinary
            if (photoToDelete.PublicId != null)
            {
                var deletionResult = await photoService.DeletePhotoAsync(photoToDelete.PublicId);
                if (deletionResult.Error != null) 
                    return BadRequest(deletionResult.Error.Message); //ako na cloudinaryu nije uspilo ne zelimo ni u db
            }

            //delete foto from db
            user.Photos.Remove(photoToDelete);

            if (await userRepository.SaveAllAsync()) 
                return Ok();

            return BadRequest("Failed to delete foto");
        }
    }
}