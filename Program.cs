using Gamidas.Utils;
using worker_email;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.ConfigureGamidas();
        services.AddHostedService<WorkerEmail>();
    })
    .Build();

host.Run();
