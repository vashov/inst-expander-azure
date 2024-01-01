using InstExpanderFunctions.FollowersWorker.Jobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace InstExpanderFunctions.FollowersWorker.Functions
{
    public class FollowerStatsJobHttpFunction
    {
        private readonly FollowerStatsJob followerStatsJob;

        public FollowerStatsJobHttpFunction(FollowerStatsJob followerStatsJob)
        {
            this.followerStatsJob = followerStatsJob;
        }

        [Function("FollowerStatsJobHttp")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData httpRequest)
        {
            await followerStatsJob.Start();

            var response = httpRequest.CreateResponse(System.Net.HttpStatusCode.OK);
            return response;
        }
    }
}
