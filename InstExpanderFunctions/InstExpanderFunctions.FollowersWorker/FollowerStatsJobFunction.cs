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
        private readonly FunctionConfiguration configuration;

        public FollowerStatsJobFunction(
            ILoggerFactory loggerFactory,
            InstagramWorker instagramWorker,
            FunctionConfiguration configuration)
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

            await StartJob("InstagramWorker", instagramWorker.StartJob);

            logger.LogInformation("FollowerStatsJobFunction finished at: {date}", DateTime.UtcNow);
        }

        private async Task StartJob(string jobName, Func<Task> job)
        {
            int countToRunJob = 1;
            bool isChallengeRequiredExceptionCatched = false;
            while (countToRunJob-- > 0)
            {
                try
                {
                    await job();
                }
                catch (ChallengeRequiredException e)
                {
                    if (isChallengeRequiredExceptionCatched)
                    {
                        logger.LogWarning("{JobName} Job was cancelled: {reason}", jobName, e.Message);
                    }
                    else
                    {
                        isChallengeRequiredExceptionCatched = true;

                        // Let's try again, maybe the Challenge was passed by a human.
                        countToRunJob++;
                        double delayMin = configuration.ChallengeRequiredDelayMin;
                        logger.LogWarning("{JobName} Job await {min} min to challenge be passed: {reason}", jobName, delayMin, e.Message);
                        await Task.Delay(TimeSpan.FromMinutes(delayMin));
                    }
                }
                catch (CancellationException e)
                {
                    logger.LogWarning("{JobName} Job was cancelled: {reason}", jobName, e.Message);
                }
            }
        }
    }
}
