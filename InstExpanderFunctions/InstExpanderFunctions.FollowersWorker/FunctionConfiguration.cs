using InstExpander.BusinessLogic.Services;
using Microsoft.Extensions.Configuration;

namespace InstExpanderFunctions.FollowersWorker
{
    public class FunctionConfiguration : ConfigurationService
    {
        public string FollowerStatsJobFunctionTimeTriggerCron { get; private set; }

        public FunctionConfiguration(IConfiguration config) : base(config)
        {
        }

        protected override List<string> GetConfigurationErrors()
        {
            List<string> errors = base.GetConfigurationErrors();

            if (string.IsNullOrWhiteSpace(FollowerStatsJobFunctionTimeTriggerCron))
                errors.Add("FollowerStatsJobFunctionTimeTriggerCron is not defined in configuration");

            return errors;
        }

        protected override void Init(IConfiguration config)
        {
            base.Init(config);

            FollowerStatsJobFunctionTimeTriggerCron = config["FollowerStatsJobFunctionTimeTriggerCron"];
        }
    }
}
