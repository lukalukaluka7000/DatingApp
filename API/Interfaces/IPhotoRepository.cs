using API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IEnumerable<Photo>> GetAllUnApprovedPhotos();

        Task<bool> ApprovePhoto(int photoId);
        Task<bool> RejectPhoto(int photoId);
       
    }
}
