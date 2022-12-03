using BazarServer.Application;
using BazarServer.Services;
using BazarServer.Infrastructure;
using MediatR;
using Microsoft.OpenApi.Models;
using System.Reflection;
using BazarServer.Application.PeerServers;

namespace BazarServer;

public class Startup
{
	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public IConfiguration Configuration { get; }

	// This method gets called by the runtime. Use this method to add services to the container.
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddHealthChecks();

		services.AddMediatR(Assembly.GetExecutingAssembly());

		services.AddSingleton(typeof(IWebSockManager), typeof(WebSockManager));

		services.UseBazarApplication(Configuration);
		services.UseBazarInfrastructure(Configuration);

		services.AddHostedService<UserQueryService>();
		services.AddHostedService<PostLangDetectService>();
		services.AddHostedService<PeerService>();

		services.AddControllers();

		services.AddCors(o => o.AddPolicy("AllowDomain", builder =>
		{
			builder.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader();
		}));

		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "BazarServer",
				Version = "v1",
				Description = "Bazar Initiative: https://bazarinitiative.org/",
				Contact = new()
				{
					Name = "bazarinitiative@yahoo.com",
					Email = "bazarinitiative@yahoo.com",
				}
			});
			c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BazarServer.Api.xml"), true);
			c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BazarServer.Application.xml"), true);
			c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BazarServer.Entity.xml"), true);
			c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "BazarServer.Infrastructure.xml"), true);
		}).AddSwaggerGenNewtonsoftSupport();

	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(
		IApplicationBuilder app,
		IWebHostEnvironment env,
		IPeerManager peerManager)
	{
		//if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseCors("AllowDomain");

		app.UseSwagger();
		app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BazarServer v1"));

		app.UseHttpsRedirection();

		//
		app.UseMiddleware<MyMiddleware>();

		//
		app.UseHealthChecks("/Health");

		// to reduce threadpool starvation
		ThreadPool.SetMinThreads(200, 50);

		//
		int cc = peerManager.GetServerCount();

		app.UseRouting();
		app.UseAuthorization();
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});

		LogFacade.LogInformation("application Configure ok");
	}

}
