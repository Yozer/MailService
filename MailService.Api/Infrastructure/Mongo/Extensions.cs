using System.Collections.Generic;
using Autofac;
using MailService.Api.Model;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace MailService.Api.Infrastructure.Mongo
{
    public static class Extensions
    {
        public static void AddMongo(this ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                var options = new MongoDbOptions();
                configuration.GetSection("mongo").Bind(options);

                return options;
            }).SingleInstance();

            builder.Register(context =>
            {
                var options = context.Resolve<MongoDbOptions>();
                BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
                BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
                ConventionRegistry.Register("Conventions", new MongoDbConventions(), x => true);

                return new MongoClient(options.ConnectionString);
            }).SingleInstance();

            builder.Register(context =>
            {
                var options = context.Resolve<MongoDbOptions>();
                var client = context.Resolve<MongoClient>();
                return client.GetDatabase(options.Database);

            }).InstancePerLifetimeScope();
        }

        public static void AddMongoRepository<TRepository, TEntity>(this ContainerBuilder builder, string collectionName)
            where TRepository : IMongoRepository<TEntity> 
            where TEntity : Entity
        {
            builder.Register(ctx =>
                {
                    return ctx.Resolve<IMongoDatabase>().GetCollection<TEntity>(collectionName);
                })
                .InstancePerLifetimeScope();
            builder
                .RegisterType<TRepository>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }

        private class MongoDbConventions : IConventionPack
        {
            public IEnumerable<IConvention> Conventions => new List<IConvention>
            {
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
                new CamelCaseElementNameConvention()
            };
        }
    }
}
