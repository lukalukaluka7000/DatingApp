using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext context;

        public LikesRepository(DataContext context)
        {
            this.context = context;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await context.Likes.FirstOrDefaultAsync(like => like.SourceUserId == sourceUserId &&
                like.LikedUserId == likedUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = context.Users.AsQueryable();
            var likes = context.Likes.AsQueryable();

            if(likesParams.Predicate == "liked")
            {
                //var user = users.FirstOrDefaultAsync(u => u.Id == userId);
                var likesOfUser = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likesOfUser.Select(x => x.LikedAppUser);
            }
            else if (likesParams.Predicate == "likedBy")
            {
                var likesOfUser = likes.Where(like => like.LikedUserId == likesParams.UserId);
                users = likesOfUser.Select(x => x.SourceAppUser);
            }

            var source = users.Select(user => new LikeDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Age = user.GetAge(),
                KnownAs = user.KnownAs,
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain == true).Url,
                City = user.City
            }).AsQueryable();

            return await API.Helpers.PagedList<LikeDto>.CreateAsync(source, likesParams.PageNumber,
                likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}
