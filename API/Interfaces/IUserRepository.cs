using API.DTOs;
using API.Entities;
using API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        Task<string> GetUserGender(string username);
        void Update(AppUser user);

        Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<AppUser> GetUserByIdAsync(int id);

        Task<AppUser> GetUserByUsernameAsync(string username);
       
        Task<MemberDTO> GetMemberAsync(string username, string currentUserName);

        Task<IEnumerable<MemberDTO>> GetMembersAsync();
        Task<PagedList<MemberDTO>> GetMembersAsync(UserParams queryParams);
    }
}
