using API.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        public UnitOfWork(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public IMessageRepository messageRepository => new MessageRepository(context, mapper);

        public IUserRepository userRepository => new UserRepository(context, mapper);

        public ILikesRepository likesRepository => new LikesRepository(context);
        public IPhotoRepository photoRepository => new PhotoRepository(context);

        public bool HasChanges()
        {
            return context.ChangeTracker.HasChanges();
        }

        public async Task<bool> Complete()
        {
            return await context.SaveChangesAsync() > 0;
            
        }
    }
}
