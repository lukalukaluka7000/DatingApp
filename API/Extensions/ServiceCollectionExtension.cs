using API.Data;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace API.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection AllServices, 
            IConfiguration configuration)
        {
            AllServices.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            AllServices.AddScoped<ITokenService, TokenService>();
            AllServices.AddScoped<IUserRepository, UserRepository>();
            AllServices.AddScoped<ILikesRepository, LikesRepository>();
            AllServices.AddScoped<IMessageRepository, MessageRepository>();
            
            AllServices.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            
            //AllServices.AddDbContext<IdentityDbContext>(options =>
            //{
            //    options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            //});
            AllServices.AddIdentityCore<AppUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddSignInManager<SignInManager<AppUser>>()
                .AddRoleValidator<RoleValidator<AppRole>>()
                .AddEntityFrameworkStores<DataContext>();

            AllServices.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            });
            AllServices.AddScoped<IPhotoService, PhotoService>();
            AllServices.AddScoped<LogUserActivity>();
            //sa jos tria injectat ovaj servis negdi
            AllServices.AddSignalR();
            AllServices.AddSingleton<PresenceTracker>();
            AllServices.AddSingleton<LiveChat>();
            return AllServices;
        }
    }
}
