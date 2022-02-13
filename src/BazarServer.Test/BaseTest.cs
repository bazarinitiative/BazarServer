using BazarServer.Application.PeerServers;
using BazarServer.Entity.PeerServers;
using BazarServer.Entity.Storage;
using BazarServer.Infrastructure.Commands;
using BazarServer.Infrastructure.PeerServers;
using BazarServer.Infrastructure.Posts;
using BazarServer.Infrastructure.Users;
using Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BazarServer
{
	public class BaseTest
	{
		protected IServiceProvider provider;

		public BaseTest()
		{
			var services = new ServiceCollection();

			services.AddSingleton<ILogger<PostRepository>>(new NullLogger<PostRepository>());
			services.AddSingleton<ILogger<UserRepository>>(new NullLogger<UserRepository>());
			services.AddSingleton<ILogger<MongoContext>>(new NullLogger<MongoContext>());
			services.AddSingleton<ILogger<PeerManager>>(new NullLogger<PeerManager>());
			services.AddSingleton<ILogger<WebSockManager>>(new NullLogger<WebSockManager>());

			var mongodb = ConfigHelper.GetConfigValue(null, "BazarMongodb");
			var config = new Mock<IConfiguration>();
			config.SetupGet(x => x["BazarMongodb"]).Returns(mongodb);
			config.SetupGet(x => x["BazarBaseUrl"]).Returns("https://localhost:5000/");

			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

			services.AddSingleton<MediatR.ISender>(new Mock<MediatR.ISender>().Object);

			services.AddSingleton<IUserRepository, UserRepository>();
			services.AddSingleton(typeof(IMongoContext), typeof(MongoContext));
			services.AddSingleton(typeof(IGenericMongoCollection<>), typeof(GenericMongoCollection<>));

			services.AddSingleton(typeof(IUserRepository), typeof(UserRepository));
			services.AddSingleton(typeof(IPostRepository), typeof(PostRepository));
			services.AddSingleton(typeof(ICommandRepository), typeof(CommandRepository));

			services.AddSingleton(typeof(IPeerManager), typeof(PeerManager));
			services.AddSingleton(typeof(IWebSockManager), typeof(WebSockManager));
			services.AddSingleton(typeof(IPeerServerRepository), typeof(PeerServerRepository));
			services.AddSingleton(typeof(IAntiSpam), typeof(AntiSpam));

			provider = services.BuildServiceProvider();
		}
	}
}