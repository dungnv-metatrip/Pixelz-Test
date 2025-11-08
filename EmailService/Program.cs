using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Register your background worker
        services.AddHostedService<EmailWorker>();
    });

var host = builder.Build();
await host.RunAsync();