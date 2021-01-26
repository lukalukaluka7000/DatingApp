using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker presenceTracker;

        public PresenceHub(PresenceTracker presenceTracker)
        {
            this.presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;
            await presenceTracker.UserConnected(username, Context.ConnectionId);
            
            await Clients.Others.SendAsync("userConnected", username); //userConnected na klijentu

            var currentUsers = await presenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers); //samo calleru na klijentu GetOnlineUsers
            //a onda klijent od tamo poziva sa trackera ovo gono iz trackera
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;
            await presenceTracker.UserDisconnected(username, Context.ConnectionId);

            await Clients.Others.SendAsync("userDisconnected", username);

            //var currentUsers = await presenceTracker.GetOnlineUsers();
            //await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

            await base.OnDisconnectedAsync(exception);
        }
        //public async Task updateUsersDictionary(string username)
        //{
        //    await presenceTracker.UserConnected(username, Context.ConnectionId);
        //}
        //public async Task updateUsersDictionaryRemove(string username)
        //{
        //    await presenceTracker.UserDisconnected(username, Context.ConnectionId);
        //}
        
    }
}
