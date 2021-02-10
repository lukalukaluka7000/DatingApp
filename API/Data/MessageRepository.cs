using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public MessageRepository(DataContext context,IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            context.Groups.Add(group);
        }
        public async Task<Connection> GetConnection(string connectionId)
        {
            return await context.Connections.FindAsync(connectionId);
        }
        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }
        public void RemoveConnection(Connection connection)
        {
            context.Connections.Remove(connection);
        }
        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await context.Groups
                .Include(c => c.Connections)
                .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }


        public void AddMessage(Message message)
        {
            this.context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages
                .Include(s => s.Sender)
                .Include(r => r.Recipient)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        

        public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context
                .Messages
                
                .OrderByDescending(m => m.MessageSent)
                .ProjectTo<MessageDTO>(mapper.ConfigurationProvider)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username &&  u.RecipientDeleted==false),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && u.DateRead == null &&
                    u.RecipientDeleted==false)
            };

            //var projectedSource = mapper.ProjectTo<MessageDTO>(query);
            //var mes = query.ProjectTo<MessageDTO>(mapper.ConfigurationProvider);
            return await API.Helpers.PagedList<MessageDTO>.CreateAsync(query, 
                messageParams.PageNumber, 
                messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = context.Messages
                //.Include(u => u.Sender).ThenInclude(p => p.Photos)
                //.Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m =>
                    m.Sender.UserName == currentUsername && m.SenderDeleted == false &&
                    m.Recipient.UserName == recipientUsername
                    ||
                    m.Sender.UserName == recipientUsername &&
                    m.Recipient.UserName == currentUsername && m.RecipientDeleted == false)
                .OrderByDescending(m => m.MessageSent);



            // messages.ForEach(m => m.DateRead = DateTime.UtcNow ? m.DateRead == null && m.RecipientUsername == currentUsername );

            // messages.ProjectTo<MessageDTO>(mapper.ConfigurationProvider);

            //for (int i = 0; i < messages.Count(); i++)
            //{
            //    if(messages[i].DateRead == null && messages[i].RecipientUsername == currentUsername)
            //    {
            //        messages[i].DateRead = DateTime.UtcNow;
            //    }
            //}


            // mark messages as read, we not projecting so include photos + tolistasync
            var unReadMessages = messages.Where(m => m.DateRead == null && 
                m.RecipientUsername == currentUsername).ToList(); // ja sam primatelj, ovo bi skuzia sam u dizajnu


            if (unReadMessages.Any())
            {
                foreach (var mess in unReadMessages)
                {
                    mess.DateRead = DateTime.UtcNow;
                }
                //sad je ovo posao za unitofwork a ne repo, pa saveamo tamo odakle zovemo, a toje u directmessageHUB
                //await context.SaveChangesAsync(); //ako se promijeni, doli return svakako vrati
            }

            //nema Project<> jer vec imam ToList
            var toreutr= await mapper.ProjectTo<MessageDTO>(messages).ToListAsync();
            return toreutr;
        }
    }
}
