using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IUnitOfWork unitOfWork;

        public AdminController(UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await userManager.Users
                .Include(ur => ur.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(s => s.UserName)
                .Select(s => new
                {
                    s.Id,
                    Username = s.UserName,
                    Roles = s.UserRoles.Select(r => r.Role.Name).ToList()
                }) .ToListAsync();
            return Ok(users);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration()
        {
            var unApprovedPhotos = await unitOfWork.photoRepository.GetAllUnApprovedPhotos();
            if (unApprovedPhotos == null)
                return BadRequest("Failed to get photos for admin to moderate on api");

            return Ok(unApprovedPhotos);
        }
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult<string>> ApprovePhoto(int photoId)
        {
            bool success = await unitOfWork.photoRepository.ApprovePhoto(photoId);
            
            if(success && await unitOfWork.Complete())
                return Ok($"Photo with id {photoId} successfuly approved");

            return BadRequest("Failed to approve photo by admin");
        }
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult<string>> RejectPhoto(int photoId)
        {
            bool rejected = await unitOfWork.photoRepository.RejectPhoto(photoId);

            if (rejected && await unitOfWork.Complete())
                return Ok($"Photo with id {photoId} successfuly rejected");

            return BadRequest("Failed to reject photo by admin");
        }
        

        // edit-roles/lisa?roles=Moderator,Member
        [Authorize(Policy = "EditUsersRoles")]
        [HttpPost("edit-roles/{usernameToEdit}")]
        public async Task<ActionResult> EditUserRoles(string usernameToEdit, string roles)
        {
            var listOfSelectedRoles = roles.Split(",");

            //userManager.FindByNameAsync()
            var userToEdit = await unitOfWork.userRepository.GetUserByUsernameAsync(usernameToEdit);
            var currentUserRoles = await userManager.GetRolesAsync(userToEdit);

            if (userToEdit == null) return BadRequest("Cannot find User");

            foreach (var role in listOfSelectedRoles)
            {
                if (!currentUserRoles.Contains(role))
                {
                    var resultAddRole = await userManager.AddToRoleAsync(userToEdit, role);
                    if (!resultAddRole.Succeeded)
                    {
                        return BadRequest("Failed to Add Roles for user");
                    }
                }
            }

            // remove not selected roles
            var resultRemoveRoles = await userManager.RemoveFromRolesAsync(userToEdit,
                currentUserRoles.Except(listOfSelectedRoles));

            if (!resultRemoveRoles.Succeeded)
            {
                return BadRequest("Failed to remove from roles!");
            }
            
            return Ok(userManager.GetRolesAsync(userToEdit));
        }
    }
}
