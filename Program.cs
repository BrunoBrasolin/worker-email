using Gamidas.Utils;
using worker_email;

HostBuilder builder = new();
builder.ConfigureAppConfiguration((hostingContext, config) =>
{
	config.SetBasePath("/app/config");
	config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
});

IHost host = builder.ConfigureServices(args)
	.ConfigureServices(services =>
	{
		services.ConfigureGamidas();
		services.AddHostedService<WorkerEmail>();
	})
	.Build();

host.Run();
