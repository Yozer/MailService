using MailService.Api.Model;

namespace MailService.Api.Infrastructure.Mongo
{
    public interface IMongoRepository<TEntity> where TEntity : Entity
    {
    }
}