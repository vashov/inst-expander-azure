|Branch|Status|
|---|---|
|main|[![Build and deploy Azure Function App](https://github.com/vashov/inst-expander-azure/actions/workflows/main.yml/badge.svg?branch=main)](https://github.com/vashov/inst-expander-azure/actions/workflows/main.yml)|

# InstExpander Azure

This is the **Azure** part of the **InstExpander** project.
There are Azure components, ARM templates and deploy scripts.
InstExpander is a bot for Instagram designed to collect and process statistics about Instagram users' followers and followings. The bot retrieves data from Instagram and sends notifications to users when someone unfollows them.

## Private NuGet packages
**InstExpander.BusinessLogic**
**InstExpander.DataAccess**

## Environment Variables
### Required
    Azure:CosmosDbConnectionString
    Azure:StorageBlobConnectionString
    Azure:StorageBlobContainerName
    InstagramSettings:Username
    InstagramSettings:Password
    InstagramSettings:BotId
    InstagramSettings:StateFileName - "state.bin"
    FollowerStatsJobFunctionTimeTriggerCron - "0 30 15 * * *"

### Optional
    TestSettings:TestUsernames - "username1,username2"
    InstagramSettings:FollowersRequestCount - 1
    InstagramSettings:InstagramRequiredOrdersCountForProcessing - 7
    InstagramSettings:InstagramOrdersCountToTakeFollowings - 3
    ProcessSettings:NewOrderDaysDelay - 0.5
    FunctionSettings:ChallengeRequiredDelayMin - 5.0

