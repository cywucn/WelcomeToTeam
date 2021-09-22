using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace WelcomeToTeam
{
    public static class SimpleMessenger
    {
        public static async Task<(string conversationId, string activityId)> SendMessageToMember(string userTeamsId, string botTeamsId, string tenantId, string serviceUrl, IActivity message, BotFrameworkHttpAdapter adapter, MicrosoftAppCredentials credentials, CancellationToken cancellationToken)
        {
            ConversationParameters conversationParameters = new ConversationParameters()
            {
                IsGroup = false,
                Bot = new ChannelAccount() { Id = botTeamsId },
                Members = new ChannelAccount[] { new ChannelAccount() { Id = userTeamsId } },
                TenantId = tenantId
            };
            string conversationId = null, activityId = null;
            await adapter.CreateConversationAsync
            (
                null,
                serviceUrl,
                credentials,
                conversationParameters,
                async (turnContext, cancellationToken) =>
                {
                    conversationId = turnContext.Activity.Conversation.Id;
                    await turnContext.SendActivityAsync(message, cancellationToken);
                    activityId = message.Id;
                },
                cancellationToken
            );
            return (conversationId, activityId);
        }

        public static async Task<(string conversationId, string activityId)> SendMessageToTeam(string teamTeamsId, string serviceUrl, IActivity message, BotFrameworkHttpAdapter adapter, MicrosoftAppCredentials credentials, CancellationToken cancellationToken)
        {
            ConversationParameters conversationParameters = new ConversationParameters()
            {
                IsGroup = true,
                ChannelData = new TeamsChannelData { Channel = new ChannelInfo { Id = teamTeamsId } },
                Activity = (Activity)message
            };
            string conversationId = null, activityId = null;
            await adapter.CreateConversationAsync
            (
                null,
                serviceUrl,
                credentials,
                conversationParameters,
                (turnContext, cancellationToken) =>
                {
                    conversationId = turnContext.Activity.Conversation.Id;
                    activityId = turnContext.Activity.Id;
                    return Task.CompletedTask;
                },
                cancellationToken
            );
            return (conversationId, activityId);
        }

        public static async Task<string> SendMessage(string conversationId, string serviceUrl, IActivity message, BotFrameworkHttpAdapter adapter, string appId, CancellationToken cancellationToken)
        {
            ConversationReference conversationReference = new ConversationReference() { Conversation = new ConversationAccount() { Id = conversationId }, ServiceUrl = serviceUrl };
            string activityId = null;
            await adapter.ContinueConversationAsync
            (
                appId,
                conversationReference,
                async (turnContext, cancellationToken) =>
                {
                    await turnContext.SendActivityAsync(message, cancellationToken);
                    activityId = message.Id;
                },
                cancellationToken
            );
            return activityId;
        }

        public static async Task UpdateMessage(string conversationId, string activityId, string serviceUrl, IActivity message, BotFrameworkHttpAdapter adapter, string appId, CancellationToken cancellationToken)
        {
            ConversationReference conversationReference = new ConversationReference() { Conversation = new ConversationAccount() { Id = conversationId }, ServiceUrl = serviceUrl };
            message.Id = activityId;
            await adapter.ContinueConversationAsync
            (
                appId,
                conversationReference,
                async (turnContext, cancellationToken) =>
                {
                    await turnContext.UpdateActivityAsync(message, cancellationToken);
                },
                cancellationToken
            );
        }

        public static async Task StartDialog(string teamId, string userId, string ssoConversationId, string ssoUserTeamsId, string serviceUrl, BotFrameworkHttpAdapter adapter, ConversationState conversationState, MainDialog dialog, string appId, CancellationToken cancellationToken)
        {
            ConversationReference conversationReference = new ConversationReference() { Conversation = new ConversationAccount() { Id = ssoConversationId }, ServiceUrl = serviceUrl };
            await adapter.ContinueConversationAsync
            (
                appId,
                conversationReference,
                async (turnContext, cancellationToken) =>
                {
                    turnContext.Activity.ChannelId = "msteams";
                    turnContext.Activity.From = new ChannelAccount() { Id = ssoUserTeamsId };
                    await conversationState.ClearStateAsync(turnContext, cancellationToken);
                    await conversationState.CreateProperty<string>("teamId").SetAsync(turnContext, teamId, cancellationToken);
                    await conversationState.CreateProperty<string>("userId").SetAsync(turnContext, userId, cancellationToken);
                    await dialog.RunAsync(turnContext, conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                },
                cancellationToken
            );
        }
    }
}
