using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailService.Api.Infrastructure.Mongo;
using MailService.Api.Model;
using MongoDB.Driver;

namespace MailService.Api.Infrastructure
{
    public class EmailsRepository : IEmailsRepository
    {
        private readonly IMongoCollection<EmailEntity> _collection;

        public EmailsRepository(IMongoCollection<EmailEntity> collection) 
            => _collection = collection;

        public IAsyncEnumerable<EmailEntity> GetEmails(EmailStatus status) =>
            _collection
                .Find(t => t.Status == status)
                .ToAsyncEnumerable();

        public IAsyncEnumerable<EmailEntity> GetAllEmails()
            => _collection
                .Find(t => true)
                .ToAsyncEnumerable();

        public Task<EmailEntity> GetEmail(Guid id)
            => _collection.Find(t => t.Id == id).SingleOrDefaultAsync();

        public Task AddNewEmail(EmailEntity emailEntity) 
            => _collection.InsertOneAsync(emailEntity);

        public Task UpdateEmail(EmailEntity emailEntity)
            => _collection.ReplaceOneAsync(t => t.Id == emailEntity.Id, emailEntity);
    }
}
