using InstExpander.BusinessLogic;
using InstExpander.BusinessLogic.Interfaces;
using InstExpanderFunctions.FollowersWorker;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddScoped<FunctionConfiguration>();
        services.AddInstagramWorker();
    })
    .Build();

var instaApiAuthorizer = host.Services.GetRequiredService<IInstaApiAuthorizer>();
instaApiAuthorizer.Authorize().Wait();

host.Run();
