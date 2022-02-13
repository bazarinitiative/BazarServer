using BazarServer.Application.Commands;
using BazarServer.Application.PeerServers;
using BazarServer.Entity.PeerServers;
using Common.Utils;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BazarServer.Application
{
	public static class Inject
	{
		public static void UseBazarApplication(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddMediatR(Assembly.GetExecutingAssembly());

			services.AddSingleton(typeof(IAntiSpam), typeof(AntiSpam));
			services.AddSingleton(typeof(IPeerManager), typeof(PeerManager));
			services.AddSingleton(typeof(ICommandManager), typeof(CommandManager));
		}
	}
}
