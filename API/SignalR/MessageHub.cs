using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly LiveChat liveChat;

        //private readonly LiveChat liveChat;

        public MessageHub(LiveChat liveChat)
        {
            this.liveChat = liveChat;
        }

        //public override async Task OnConnectedAsync()
        //{
        //    Debug.WriteLine("New connection on message hub");
        //    //var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;

        //    var currentMessages = liveChat.GetLiveMessages();
        //    await Clients.All.SendAsync("GetLiveMessagesInClient", currentMessages); //hubConnection.on("GetLiveMessagesInClient")
        //}
        //public override async Task OnDisconnectedAsync(Exception exception)
        //{
        //    //await base.OnDisconnectedAsync(exception);
        //}
        public async override Task OnConnectedAsync()
        {
            var currentMessages = liveChat.GetLiveMessages();
            await Clients.All.SendAsync("GetLiveMessagesInClient", currentMessages);

        }
        public async override Task OnDisconnectedAsync(Exception ex)
        {
            await base.OnDisconnectedAsync(ex);
        }
        public async Task SendDirectMessage(string message)
        {
            var broadcasterUsername = Context.User.FindFirst(ClaimTypes.Name)?.Value;

            await this.liveChat.SendDirectMessage(message, broadcasterUsername);


        }
        //private Task UpdateLiveMessages(string message)
        //{
        //    //var userInfoSender = _userInfoInMemory.GetUserInfo(Context.User.Identity.Name);
        //    //var userInfoReciever = _userInfoInMemory.GetUserInfo(targetUserName);


        //    lock (LiveMessages)
        //    {
        //        LiveMessages.Add(message, "user");
        //    }


        //    return Task.CompletedTask;
        //    //return Task.CompletedTask;
        //    //return Clients.Client(userInfoReciever.ConnectionId).SendAsync("SendDM", message, userInfoSender);
        //}

        //private Task<string[]> GetLiveMessages()
        //{
        //    string[] liveMessages;

        //    lock (LiveMessages)
        //    {
        //        liveMessages = LiveMessages.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
        //    }

        //    return Task.FromResult(liveMessages);
        //}

    }
}
