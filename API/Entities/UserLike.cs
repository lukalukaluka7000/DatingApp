using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
    public class UserLike
    {
        public int SourceUserId { get; set; }
        public AppUser SourceAppUser{ get; set; }
        public int LikedUserId { get; set; }
        public AppUser LikedAppUser{ get; set; }
    }
}
