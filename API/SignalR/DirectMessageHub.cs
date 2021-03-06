﻿using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class DirectMessageHub : Hub
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly PresenceTracker tracker;
        private readonly IHubContext<PresenceHub> presenceHub;

        public DirectMessageHub(IUnitOfWork unitOfWork, IMapper mapper,
            PresenceTracker tracker,
            IHubContext<PresenceHub> presenceHub)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.tracker = tracker;
            this.presenceHub = presenceHub;
        }

        public async override Task OnConnectedAsync()
        {
            var usernameFirst = Context.User.FindFirst(ClaimTypes.Name)?.Value;
            var httpContext = Context.GetHttpContext();
            var usernameSecond = httpContext.Request.Query["user"].ToString();

            //groups alphabetical member names divides by underscore
            string groupName = ConstructGroupName(usernameFirst, usernameSecond);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var returnedUpdatedGroupWithConnection = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("groupUpdated", returnedUpdatedGroupWithConnection);

            var messages = await unitOfWork.messageRepository.GetMessageThread(usernameFirst, usernameSecond);

            if (unitOfWork.HasChanges()) // ovo govno ne tracka nista, potrosia sam 2 sata da nadem sta je problem sa unread porukama, druge instance contexta nema, jednostavno fucking vraca false
            {
                bool uspjeh = await unitOfWork.Complete();
            }
           
            //now pass those messages to the group i onda tamo u hubu u angularu to docekas
            await Clients.Groups(groupName).SendAsync("receiveCurrentDMs", messages);

            //ne znam kako s ovim ista postzien
            //await Clients.Caller.SendAsync("receiveCurrentDMs", messages);


        }
        public async override Task OnDisconnectedAsync(Exception ex)
        {
            var returnedUpdatedGroupWithDeletedConnection =  await RemoveFromMessageGroup();
            await Clients.Group(returnedUpdatedGroupWithDeletedConnection.Name).SendAsync("groupUpdated", returnedUpdatedGroupWithDeletedConnection);
            await base.OnDisconnectedAsync(ex);
        }
        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var senderUsername = Context.User.FindFirst(ClaimTypes.Name)?.Value;
            if (senderUsername == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot send messages to yourself!");
            var senderUser = await unitOfWork.userRepository.GetUserByUsernameAsync(senderUsername);

            var recipientUser = await unitOfWork.userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if (recipientUser == null) throw new HubException("User not found");

            var message = new Message
            {
                Sender = senderUser,
                SenderDeleted = false,
                SenderId = senderUser.Id,
                SenderUsername = senderUser.UserName,
                Recipient = recipientUser,
                RecipientDeleted = false,
                RecipientId = recipientUser.Id,
                RecipientUsername = recipientUser.UserName,
                Content = createMessageDto.Content,
            };
            
            

            var groupName = ConstructGroupName(senderUsername, recipientUser.UserName);
            var group = await unitOfWork.messageRepository.GetMessageGroup(groupName);

            // ako postoji konekcija, a ako ne kreiraj je
            if (group.Connections.Any(x => x.Username == recipientUser.UserName))
            {
                message.DateRead = DateTime.UtcNow;

            }
            else
            {
                var connections = await tracker.GetConnectionsForUser(recipientUser.UserName);
                //ako recipient nije konektan na dm hub
                if (connections != null)
                {
                    await presenceHub.Clients.Clients(connections).SendAsync("newMessageReceivedNotification",
                        new
                        {
                            Username = senderUsername,
                            KnownAs = senderUser.KnownAs
                        });
                }
            }

            unitOfWork.messageRepository.AddMessage(message);
            //await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            if (await unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("newMessageReceived", mapper.Map<MessageDTO>(message));
            }
            
        }
        private string ConstructGroupName(string first, string second)
        {
            var comparer = string.CompareOrdinal(first, second);
            return comparer < 0 ? $"{first}_{second}" : $"{second}_{first}";
        }
        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await unitOfWork.messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.FindFirst(ClaimTypes.Name)?.Value);
            if (group == null)
            {
                group = new Group(groupName);
                unitOfWork.messageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);

            if(await unitOfWork.Complete())
            {
                return group;
            }

            throw new HubException("Failed to join group");
        }
        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await unitOfWork.messageRepository.GetGroupForConnection(Context.ConnectionId);
            if (group != null)
            {
                var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
                unitOfWork.messageRepository.RemoveConnection(connection);
                if(await unitOfWork.Complete())
                {
                    return group;
                }
            }
            throw new HubException("Failed to remove from group");

        }
    }
}
