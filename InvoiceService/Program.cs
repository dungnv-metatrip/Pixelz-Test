using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // Register your background worker
        services.AddHostedService<InvoiceWorker>();
    });

var host = builder.Build();
await host.RunAsync();