# Environment Variables
    Azure:CosmosDbConnectionString
    Azure:StorageBlobConnectionString
    Azure:StorageBlobContainerName
    InstagramSettings:Username
    InstagramSettings:Password
    InstagramSettings:BotId
    InstagramSettings:StateFileName - "state.bin"
    FollowerStatsJobFunctionTimeTriggerCron - "0 30 15 * * *"

# Optional Environment Variables
    TestSettings:TestUsernames - "username1,username2"
    InstagramSettings:FollowersRequestCount - 1
    InstagramSettings:InstagramRequiredOrdersCountForProcessing - 7
    InstagramSettings:InstagramOrdersCountToTakeFollowings - 3
    ProcessSettings:NewOrderDaysDelay - 0.5
    FunctionSettings:ChallengeRequiredDelayMin - 5.0
