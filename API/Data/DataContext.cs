using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int, 
        IdentityUserClaim<int>, AppUserRole,
        IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext( DbContextOptions options) : base(options)
        {
        }

        //public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages{ get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.User)
                .HasForeignKey(f => f.UserId)
                .IsRequired();
            builder.Entity<AppRole>()
                .HasMany(ur => ur.UserRoles)
                .WithOne(u => u.Role)
                .HasForeignKey(r => r.RoleId)
                .IsRequired();

            builder.Entity<UserLike>()
                .HasKey(k => new { k.SourceUserId, k.LikedUserId });

            builder.Entity<UserLike>() 
                .HasOne(s => s.SourceAppUser)
                .WithMany(u => u.LikedUsers) //source user can like many other 
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade); //delete user delete related entities

            builder.Entity<UserLike>()
                .HasOne(s => s.LikedAppUser)
                .WithMany(u => u.LikedByUsers)
                .HasForeignKey(s => s.LikedUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(u => u.MessagesRecieved)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(u => u.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

            
                
        }
    }
}