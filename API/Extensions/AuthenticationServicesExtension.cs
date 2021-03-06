﻿

using API.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class AuthenticationServicesExtension
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection AllServices, IConfiguration configuration)
        {
            AllServices.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(configuration["TokenKey"])),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    //set signalR to use query string to pass jwt, unlike api controlers
                    options.Events = new JwtBearerEvents
                    {
                        
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs"))
                                context.Token = accessToken;

                            return Task.CompletedTask;
                        }
                    };

                });
            AllServices.AddAuthorization(opt =>
            {
                opt.AddPolicy("RequireAdminRole", opt => opt.RequireRole(new List<string> { "Admin" }));
                opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole(new List<string> { "Moderator", "Admin" }));
                opt.AddPolicy("EditUsersRoles", policy => policy.RequireRole(new List<string> { "Admin" }));
            });
            return AllServices;
        }

        //public static string Format<T>(this T Dani) where T : Enum
        //{
        //    return "Jebali te dani";
        //}

    }
}
