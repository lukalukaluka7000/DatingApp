using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.SignalR;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IHubContext<MessageHub> messageHubContext;

        public MessagesController(IUnitOfWork unitOfWork,
            IMapper mapper, IHubContext<MessageHub> messageHubContext)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.messageHubContext = messageHubContext;
        }

        [HttpPost("servermessage")]
        public IActionResult Post()
        {
            //Broadcast message to client  
            messageHubContext.Clients.All.SendAsync("Send", "Hello from the hub server at " +
                DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage([FromBody] CreateMessageDto createMessageDto)
        {
            var senderUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            if (senderUsername == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You cannot send messages to yourself!");
            var senderUser = await unitOfWork.userRepository.GetUserByUsernameAsync(senderUsername);

            var recipientUser = await unitOfWork.userRepository.GetUserByUsernameAsync( createMessageDto.RecipientUsername );
            if (recipientUser == null)  return NotFound();

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
                //DateRead optional
                //DateSent je Now
                //id je autoincrement
            };

            unitOfWork.messageRepository.AddMessage(message);

            if (await unitOfWork.Complete())
                return Ok(mapper.Map<MessageDTO>(message));

            return BadRequest("Failed to send a message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser([FromQuery]
            MessageParams messageParams)
        {
            messageParams.Username = User.FindFirst(ClaimTypes.Name)?.Value;
            messageParams.PageSize = 15;

            var messages = await unitOfWork.messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize,
                messages.TotalNumItems, messages.TotalPages);

            return Ok(messages);
        }
        // ne koristi se ova vise, SignalR
        //[HttpGet("thread/{username}")]
        //public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesThread(string username)
        //{
        //    var senderUsername = User.FindFirst(ClaimTypes.Name)?.Value;

        //    var messages = await unitOfWork.messageRepository.GetMessageThread(senderUsername, username);

        //    return Ok(messages);
        //}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var currentUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            var message = await unitOfWork.messageRepository.GetMessage(id);

            if (message.Sender.UserName != currentUsername && message.Recipient.UserName != currentUsername)
                return Unauthorized();
            if (message.Sender.UserName == currentUsername) message.SenderDeleted = true;
            if (message.Recipient.UserName == currentUsername) message.RecipientDeleted = true;

            if(message.SenderDeleted && message.RecipientDeleted)
            {
                unitOfWork.messageRepository.DeleteMessage(message);
                if (await unitOfWork.Complete())
                    return Ok();
            }
            if (message.SenderDeleted || message.RecipientDeleted)
                return Ok();
            return BadRequest("Failed to delete message");
        }
    }
}
