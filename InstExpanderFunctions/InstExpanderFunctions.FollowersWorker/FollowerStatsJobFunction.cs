using InstExpander.BusinessLogic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace InstExpanderFunctions.FollowersWorker
{
    public class FollowerStatsJobFunction
    {
        private readonly ILogger logger;
        private readonly InstagramWorker instagramWorker;

        public FollowerStatsJobFunction(
            ILoggerFactory loggerFactory,
            InstagramWorker instagramWorker)
        {
            this.logger = loggerFactory.CreateLogger<FollowerStatsJobFunction>();
            this.instagramWorker = instagramWorker;
        }

        [FixedDelayRetry(-1, "00:05:00")]
        [Function("FollowerStatsJobFunction")]
        public async Task Run([TimerTrigger("%FollowerStatsJobFunctionTimeTriggerCron%")] TimerInfo timer)
        {
            logger.LogInformation("FollowerStatsJobFunction start execution at: {date}", DateTime.UtcNow);

            if (timer.ScheduleStatus is not null)
            {
                logger.LogInformation("Next timer schedule at: {date}", timer.ScheduleStatus.Next.ToUniversalTime());
            }

            await instagramWorker.StartJob();

            logger.LogInformation("FollowerStatsJobFunction finished at: {date}", DateTime.UtcNow);
        }
    }
}
