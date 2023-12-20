using Microsoft.Extensions.Configuration;

namespace InstExpanderFunctions.FollowersWorker
{
    public class FunctionConfiguration
    {
        public double ChallengeRequiredDelayMin { get; private set; }

        public FunctionConfiguration(IConfiguration config) 
        {
            string challengeRequiredDelayMinString = config["FunctionSettings:ChallengeRequiredDelayMin"];
            ChallengeRequiredDelayMin = string.IsNullOrWhiteSpace(challengeRequiredDelayMinString) ? 5 : double.Parse(challengeRequiredDelayMinString);
        }
    }
}
