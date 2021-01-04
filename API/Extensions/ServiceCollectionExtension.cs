using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using AutoMapper;
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
            AllServices.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            AllServices.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            });
            AllServices.AddScoped<IPhotoService, PhotoService>();
            //sa jos tria injectat ovaj servis negdi
            return AllServices;
        }
    }
}
