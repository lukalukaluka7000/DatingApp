using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper mapper;

        public UserRepository(DataContext context,
            IMapper mapper)
        {
            _context = context;
            this.mapper = mapper;
        }


        public async Task<MemberDTO> GetMemberAsync(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDTO>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<MemberDTO>> GetMembersAsync()
        {
            return await _context.Users
                .ProjectTo<MemberDTO>(mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PagedList<MemberDTO>> GetMembersAsync(UserParams queryParams)
        {
            var items_svi = _context.Users.AsNoTracking().AsQueryable();
            var query = items_svi.Where(u => u.UserName != queryParams.CurrentUserName);
            if (queryParams.Gender != null)
            {
                query = query.Where(u => u.Gender == queryParams.Gender);
            }
            else
            {
                var user = await GetUserByUsernameAsync(queryParams.CurrentUserName);
                query = query.Where(u => u.Gender != user.Gender);
            }
            
            var mindob = DateTime.Today.AddYears(-queryParams.MaxAge-1);
            var maxdob = DateTime.Today.AddYears(-queryParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= mindob && u.DateOfBirth <= maxdob);

            query = queryParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };

            var items = await API.Helpers.PagedList<MemberDTO>.CreateAsync(
                query.ProjectTo<MemberDTO>(mapper.ConfigurationProvider).AsNoTracking(), 
                queryParams.PageNumber, queryParams.PageSize);
            return items;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<string> GetUserGender(string username)
        {
            return await _context.Users
                .Where(x => x.UserName == username)
                .Select(x => x.Gender)
                .FirstOrDefaultAsync();
                
        }

        public async  Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }


        
    }
}
