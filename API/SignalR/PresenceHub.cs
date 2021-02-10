using API.Extensions;
using API.Interfaces;
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
        private readonly IUnitOfWork unitOfWork;

        public PresenceHub(PresenceTracker presenceTracker, IUnitOfWork uow)
        {
            this.presenceTracker = presenceTracker;
            this.unitOfWork = uow;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;
            await presenceTracker.UserConnected(username, Context.ConnectionId);

            var appUser = await unitOfWork.userRepository.GetUserByUsernameAsync(username);
            var messagesUnreadReceived = appUser.MessagesRecieved.Where(x => x.DateRead == null).Count();

            await Clients.All.SendAsync("userConnected", username, messagesUnreadReceived);
            //await Clients.Others.SendAsync("userConnected", username, messagesUnreadReceived); //userConnected na klijentu

            
            var currentUsers = await presenceTracker.GetOnlineUsers();
            List<int> numbersUnreadMsgs = new List<int>();
            foreach(var appConnectedUsername in currentUsers) // in current connected users
            {
                var user = await unitOfWork.userRepository.GetUserByUsernameAsync(appConnectedUsername);
                numbersUnreadMsgs.Add( appUser.MessagesRecieved.Where(x => x.DateRead == null).Count() );
            }
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers, numbersUnreadMsgs); //samo calleru na klijentu GetOnlineUsers
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
