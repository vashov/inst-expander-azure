using Functions.Worker.ContextAccessor;
using InstExpander.BusinessLogic;
using InstExpander.BusinessLogic.Exceptions;
using InstExpander.BusinessLogic.Interfaces;
using Microsoft.Extensions.Logging;

namespace InstExpanderFunctions.FollowersWorker.Jobs
{
    public class FollowerStatsJob
    {
        private readonly ILogger<FollowerStatsJob> logger;
        private readonly IFunctionContextAccessor context;
        private readonly IInstaApiAuthorizer instaApiAuthorizer;
        private readonly IFileStorage fileStorage;
        private readonly FunctionConfiguration configuration;
        private readonly InstagramWorker instagramWorker;

        public FollowerStatsJob(
            ILoggerFactory loggerFactory,
            IFunctionContextAccessor context,
            IInstaApiAuthorizer instaApiAuthorizer,
            IFileStorage fileStorage,
            FunctionConfiguration configuration,
            InstagramWorker instagramWorker
            )
        {
            logger = loggerFactory.CreateLogger<FollowerStatsJob>();
            this.context = context;
            this.instaApiAuthorizer = instaApiAuthorizer;
            this.fileStorage = fileStorage;
            this.configuration = configuration;
            this.instagramWorker = instagramWorker;
        }

        public async Task Start()
        {
            string functionName = context.FunctionContext.FunctionDefinition.Name;

            logger.LogInformation("FollowerStatsJob {Function} start execution at: {Date}", functionName, DateTime.UtcNow);

            try
            {
                await instaApiAuthorizer.Authorize();
                await instagramWorker.StartJob();
            }
            catch (WaitBeforeTryAgainException)
            {
                // Delete instagram auth file to login again in next time.
                await fileStorage.DeleteIfExist(configuration.InstagramStateFileName);
                throw;
            }

            logger.LogInformation("FollowerStatsJob {Function} finished at: {Date}", functionName, DateTime.UtcNow);
        }
    }
}
