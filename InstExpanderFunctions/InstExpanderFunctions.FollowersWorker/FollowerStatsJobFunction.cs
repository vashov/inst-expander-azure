using InstExpander.BusinessLogic;
using InstExpander.BusinessLogic.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InstExpanderFunctions.FollowersWorker
{
    public class FollowerStatsJobFunction
    {
        private readonly ILogger logger;
        private readonly InstagramWorker instagramWorker;
        private readonly IConfiguration configuration;

        public FollowerStatsJobFunction(
            ILoggerFactory loggerFactory,
            InstagramWorker instagramWorker,
            IConfiguration configuration)
        {
            this.logger = loggerFactory.CreateLogger<FollowerStatsJobFunction>();
            this.instagramWorker = instagramWorker;
            this.configuration = configuration;
        }

        [Function("FollowerStatsJobFunction")]
        public async Task Run([TimerTrigger("%FollowerStatsJobFunctionTimeTriggerCron%")] TimerInfo myTimer)
        {
            logger.LogInformation("FollowerStatsJobFunction start execution at: {date}", DateTime.UtcNow);

            if (myTimer.ScheduleStatus is not null)
            {
                logger.LogInformation("Next timer schedule at: {date}", myTimer.ScheduleStatus.Next.ToUniversalTime());
            }

            try
            {
                await instagramWorker.StartJob();
            }
            catch (CancellationException e)
            {
                logger.LogWarning("InstagramWorker Job was cancelled: {reason}", e.Message);
            }

            logger.LogInformation("FollowerStatsJobFunction finished at: {date}", DateTime.UtcNow);
        }
    }
}
