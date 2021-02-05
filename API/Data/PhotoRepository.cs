using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext context;

        public PhotoRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<bool> ApprovePhoto(int photoId)
        {
            //var usernameToApprove = GetUsernameByPhoto(photoId);
            var photo = await GetPhoto(photoId);

            if (photo == null)
                return false;

            if (await context.Photos
                    .Where(f => f.AppUser.UserName == photo.AppUser.UserName)
                    .AllAsync((f => f.IsMain == false)))
                photo.IsMain = true;

            photo.IsApproved = true;

            return true;
        }
        public async Task<bool> RejectPhoto(int photoId)
        {
            var photo = await GetPhoto(photoId);

            if (photo == null)
                return false;

            context.Photos.Remove(photo);

            return true;
        }

        public async Task<IEnumerable<Photo>> GetAllUnApprovedPhotos()
        {
            return await context.Photos.Where(p => p.IsApproved == false).ToListAsync();
        }

        private async Task<string> GetUsernameByPhoto(int photoId)
        {
            var photo = await context.Photos
                .Include(x => x.AppUser)
                .FirstOrDefaultAsync(p => p.Id == photoId);
            
            return photo.AppUser.UserName;
        }

        private async Task<Photo> GetPhoto(int photoId)
        {
            return await context.Photos
                .Include(x => x.AppUser)
                .FirstOrDefaultAsync(p => p.Id == photoId);
        }

    }
}
