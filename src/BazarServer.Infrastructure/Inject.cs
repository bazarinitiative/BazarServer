using BazarServer.Entity.PeerServers;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Commands;
using BazarServer.Infrastructure.PeerServers;
using BazarServer.Infrastructure.Posts;
using BazarServer.Infrastructure.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BazarServer.Infrastructure
{
	public static class Inject
	{
		public static void UseBazarInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{

			services.AddSingleton(typeof(IMongoContext), typeof(MongoContext));
			services.AddSingleton(typeof(IGenericMongoCollection<>), typeof(GenericMongoCollection<>));

			services.AddSingleton(typeof(IUserRepository), typeof(UserRepository));
			services.AddSingleton(typeof(IPostRepository), typeof(PostRepository));
			services.AddSingleton(typeof(ICommandRepository), typeof(CommandRepository));

			services.AddSingleton(typeof(IPeerServerRepository), typeof(PeerServerRepository));
		}
	}
}
