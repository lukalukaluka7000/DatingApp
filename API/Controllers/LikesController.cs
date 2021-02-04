using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;

        public LikesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> LikeOtherUser(string username)
        {
            var UserName = User.FindFirst(ClaimTypes.Name)?.Value;
            var UserSource = await unitOfWork.userRepository.GetUserByUsernameAsync(UserName);

            var UserLiked = unitOfWork.userRepository.GetUserByUsernameAsync(username);
            var UserSourceWithLikes = await unitOfWork.likesRepository.GetUserWithLikes(UserSource.Id);

            if(UserSourceWithLikes == null) return BadRequest("No source user");
            
            if(UserLiked == null) return NotFound();
            
            if (UserSource.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await unitOfWork.likesRepository.GetUserLike(UserSourceWithLikes.Id, UserLiked.Result.Id);
            if (userLike != null) return BadRequest("You already liked this user");

            UserSourceWithLikes.LikedUsers.Add(new UserLike
            {
                SourceUserId = UserSource.Id,
                LikedUserId = UserLiked.Result.Id
            });
            if(await unitOfWork.SaveChanges()) return Ok();

            return BadRequest("Failed to like user");
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            

            var likes = await unitOfWork.likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(likes.CurrentPage, likes.PageSize,
                likes.TotalNumItems, likes.TotalPages);

            return Ok(likes);
            
        }
    }
}
