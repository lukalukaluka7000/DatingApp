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
            var query = context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username && u.RecipientDeleted==false),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username && u.SenderDeleted == false),
                _ => query.Where(u => u.Recipient.UserName == messageParams.Username && u.DateRead == null &&
                    u.RecipientDeleted==false)
            };
            var projectedSource = mapper.ProjectTo<MessageDTO>(query);
            //var mes = query.ProjectTo<MessageDTO>(mapper.ConfigurationProvider);
            return await API.Helpers.PagedList<MessageDTO>.CreateAsync(projectedSource, 
                messageParams.PageNumber, 
                messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m => 
                    m.Sender.UserName == currentUsername && m.SenderDeleted==false &&
                    m.Recipient.UserName == recipientUsername 
                    ||
                    m.Sender.UserName == recipientUsername && 
                    m.Recipient.UserName == currentUsername && m.RecipientDeleted == false)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();

            // mark messages as read, we not projecting so include photos + tolistasync
            var unReadMessages = messages.Where(m => m.DateRead == null && 
                m.Recipient.UserName == currentUsername).ToList(); // ja sam primatelj, ovo bi skuzia sam u dizajnu

            if (unReadMessages.Any())
            {
                foreach (var mess in unReadMessages)
                {
                    mess.DateRead = DateTime.Now;
                }
                await context.SaveChangesAsync(); //ako se promijeni, doli return svakako vrati
            }

            //nema Project<> jer vec imam ToList
            return mapper.Map<IEnumerable<MessageDTO>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}
