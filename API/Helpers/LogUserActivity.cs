using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (!resultContext.HttpContext.User.Identity.IsAuthenticated)
            {
                return;
            }
            //var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var user = await userRepository.GetUserByUsernameAsync(username);
            string username = resultContext.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            int id = int.Parse( resultContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            //service locator pattern
            var uow = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();
            var user = await uow.userRepository.GetUserByIdAsync(id);
            user.LastActive = DateTime.Now;
            await uow.SaveChanges();
        }
    }
}
