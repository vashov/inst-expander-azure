using InstExpander.BusinessLogic;
using InstExpanderFunctions.FollowersWorker;
using InstExpanderFunctions.FollowersWorker.Middlewares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(workerApplication =>
    {
        workerApplication.UseMiddleware<ExceptionHandlingMiddleware>();
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddScoped<FunctionConfiguration>();
        services.AddInstagramWorker();
    })
    .ConfigureLogging(logging =>
    {
        // Need to remove default filter for ApplicationInsights. Because filter is Warning level.
        // https://github.com/Azure/azure-functions-dotnet-worker/issues/1182
        // https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#application-insights
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    .Build();

host.Run();
