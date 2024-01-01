using InstExpanderFunctions.FollowersWorker.Jobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace InstExpanderFunctions.FollowersWorker.Functions
{
    public class FollowerStatsJobTimerFunction
    {
        private readonly ILogger<FollowerStatsJobTimerFunction> logger;

        private readonly FollowerStatsJob followerStatsJob;

        public FollowerStatsJobTimerFunction(
            FollowerStatsJob followerStatsJob,
            ILoggerFactory loggerFactory
            )
        {
            logger = loggerFactory.CreateLogger<FollowerStatsJobTimerFunction>();
            this.followerStatsJob = followerStatsJob;
        }

        [ExponentialBackoffRetry(-1, "00:10:00", "00:30:00")]
        [Function("FollowerStatsJobScheduled")]
        public async Task Run([TimerTrigger("%FollowerStatsJobFunctionTimeTriggerCron%")] TimerInfo timer)
        {
            if (timer.ScheduleStatus is not null)
            {
                logger.LogInformation("FollowerStatsJobScheduled Invoked: Next timer schedule at: {date}", timer.ScheduleStatus.Next.ToUniversalTime());
            }

            await followerStatsJob.Start();
        }
    }
}
