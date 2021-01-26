using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class LiveChat
    {
        private static readonly Dictionary<string, List<string>> LiveMessages =
            new Dictionary<string, List<string>>(); // key: msg, value: username

        private readonly IHubContext<MessageHub> messageHub;

        public LiveChat(IHubContext<MessageHub> messageHub)
        {
            this.messageHub = messageHub;
        }
        public Task Send(string message)
        {
            return messageHub.Clients.All.SendAsync("Send", message);
        }
        public async Task SendDirectMessage(string message, string broadcasterUsername)
        {
            //var currentMessages = GetLiveMessages();
            var currentMessages = LiveMessages;

            await messageHub.Clients.All.SendAsync("GetLiveMessagesInClient", currentMessages);

            await UpdateLiveMessages(message, broadcasterUsername);
        }
        public Task UpdateLiveMessages(string message, string broadcasterUsername)
        {
            //var userInfoSender = _userInfoInMemory.GetUserInfo(Context.User.Identity.Name);
            //var userInfoReciever = _userInfoInMemory.GetUserInfo(targetUserName);
            
            lock (LiveMessages)
            {
                if (LiveMessages.ContainsKey(broadcasterUsername))
                {
                    LiveMessages[broadcasterUsername].Add(message);
                }
                else
                {
                    LiveMessages.Add(broadcasterUsername, new List<string>() { message });
                }
            }


            return Task.CompletedTask;
            //return Task.CompletedTask;
            //return Clients.Client(userInfoReciever.ConnectionId).SendAsync("SendDM", message, userInfoSender);
        }

        public Task<Dictionary<string,List<string>>> GetLiveMessages()
        {
            //Dictionary<string, string[]> liveMessages = new Dictionary<string, string[]>();
            ////string[] liveMessages;
            //lock (LiveMessages)
            //{
                
            //   liveMessages = LiveMessages.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            //}

            return Task.FromResult(LiveMessages);
        }
    }
}
