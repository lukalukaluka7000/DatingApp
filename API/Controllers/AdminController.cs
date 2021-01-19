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
        private readonly IUserRepository userRepository;

        public AdminController(UserManager<AppUser> userManager,
            IUserRepository userRepository)
        {
            this.userManager = userManager;
            this.userRepository = userRepository;
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
                }).ToListAsync();
            return Ok(users);
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult<string> GetPhotosForModeration()
        {
            return Ok("Only moderators and admins can see this");
        }

        // edit-roles/lisa?roles=Moderator,Member
        [Authorize(Policy = "EditUsersRoles")]
        [HttpPost("edit-roles/{usernameToEdit}")]
        public async Task<ActionResult> EditUserRoles(string usernameToEdit, string roles)
        {
            var listOfSelectedRoles = roles.Split(",");

            //userManager.FindByNameAsync()
            var userToEdit = await userRepository.GetUserByUsernameAsync(usernameToEdit);
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
