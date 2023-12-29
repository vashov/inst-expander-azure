using InstExpander.BusinessLogic;
using InstExpander.BusinessLogic.Exceptions;
using InstExpander.BusinessLogic.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace InstExpanderFunctions.FollowersWorker
{
    public class FollowerStatsJobFunction
    {
        private readonly ILogger<FollowerStatsJobFunction> logger;
        private readonly IInstaApiAuthorizer instaApiAuthorizer;
        private readonly IFileStorage fileStorage;
        private readonly FunctionConfiguration configuration;
        private readonly InstagramWorker instagramWorker;

        public FollowerStatsJobFunction(
            ILoggerFactory loggerFactory,
            IInstaApiAuthorizer instaApiAuthorizer,
            IFileStorage fileStorage,
            FunctionConfiguration configuration,
            InstagramWorker instagramWorker)
        {
            this.logger = loggerFactory.CreateLogger<FollowerStatsJobFunction>();
            this.instaApiAuthorizer = instaApiAuthorizer;
            this.fileStorage = fileStorage;
            this.configuration = configuration;
            this.instagramWorker = instagramWorker;
        }

        [ExponentialBackoffRetry(-1, "00:10:00", "00:30:00")]
        [Function("FollowerStatsJobFunction")]
        public async Task Run([TimerTrigger("%FollowerStatsJobFunctionTimeTriggerCron%")] TimerInfo timer)
        {
            logger.LogInformation("FollowerStatsJobFunction start execution at: {date}", DateTime.UtcNow);

            if (timer.ScheduleStatus is not null)
            {
                logger.LogInformation("Next timer schedule at: {date}", timer.ScheduleStatus.Next.ToUniversalTime());
            }

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

            logger.LogInformation("FollowerStatsJobFunction finished at: {date}", DateTime.UtcNow);
        }
    }
}
