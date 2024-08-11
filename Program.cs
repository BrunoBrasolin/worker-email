using Gamidas.Utils;
using Serilog.Events;
using Serilog;
using worker_email;

HostBuilder builder = new();
builder.ConfigureAppConfiguration((hostingContext, config) =>
{
	config.SetBasePath("/app/config");
	config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
});

string connectionString = "";

IHost host = builder.ConfigureServices((hostContext, services) =>
	{
	connectionString = hostContext.Configuration.GetConnectionString("Default");

		services.ConfigureGamidas();
		services.AddHostedService<WorkerEmail>();
}).Build();

Log.Logger = new LoggerConfiguration()
	.Enrich.WithProperty("ApplicationName", "WORKER-Email")
	.MinimumLevel.Warning()
	.MinimumLevel.Override("worker_email", LogEventLevel.Information)
	.WriteTo.Oracle(cfg => cfg.WithSettings(connectionString)
		.UseBurstBatch()
		.CreateSink())
	.CreateLogger();

host.Run();
