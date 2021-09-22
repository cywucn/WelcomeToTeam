using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;



namespace WelcomeToTeam
{
    public class MainDialog : ComponentDialog
    {
        private readonly string _appId;
        private readonly string _appPassword;
        private readonly MicrosoftAppCredentials _credentials;
        private readonly BotFrameworkHttpAdapter _adapter;
        private readonly string _connectionName;

        private readonly ConversationState _conversationState;

        public MainDialog(IConfiguration configuration, BotFrameworkHttpAdapter adapter, ConversationState conversationState)
            : base(nameof(MainDialog))
        {

            _appId = configuration["MicrosoftAppId"];
            _appPassword = configuration["MicrosoftAppPassword"];
            _credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            _connectionName = configuration["ConnectionName"];

            _adapter = adapter;

            _conversationState = conversationState;

            AddDialog
            (
                new OAuthPrompt
                (
                    nameof(OAuthPrompt),
                    new OAuthPromptSettings
                    {
                        ConnectionName = _connectionName,
                        Timeout = 300000
                    }
                )
            );

            AddDialog
            (
                new WaterfallDialog
                (
                    nameof(WaterfallDialog),
                    new WaterfallStep[]
                    {
                        PromptStepAsync,
                        LoginStepAsync
                    }
                )
            );

            InitialDialogId = nameof(WaterfallDialog);
        }



        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                string teamId = await _conversationState.CreateProperty<string>("teamId").GetAsync(stepContext.Context, cancellationToken: cancellationToken);
                string userId = await _conversationState.CreateProperty<string>("userId").GetAsync(stepContext.Context, cancellationToken: cancellationToken);
                if (teamId is null && userId is null)
                {
                    string ssoConversationId = stepContext.Context.Activity.Conversation.Id;
                    string ssoUserTeamsId = stepContext.Context.Activity.From.Id;
                    SimpleDatabase.UpdateSSOConversationIdUserTeamsId(ssoConversationId, ssoUserTeamsId);
                    _ = stepContext.Context.SendActivityAsync("SSO is configured.", cancellationToken: cancellationToken);
                }
                else
                {
                    var graphClient = SimpleGraphClient.GetAuthenticatedClient(tokenResponse.Token);
                    string serviceUrl = stepContext.Context.Activity.ServiceUrl;
                    IActivity message = SimpleCard.Message(await SimpleCard.CommentCard(graphClient, teamId, userId));
                    (string conversationId, string activityId) = SimpleDatabase.GetSelfIntroConversationActivityId(teamId, userId);
                    if (conversationId is null)
                    {
                        string teamTeamsId = SimpleDatabase.GetTeamTeamsId(teamId);
                        _ = Task.Run(
                            async () =>
                            {
                                (conversationId, activityId) = await SimpleMessenger.SendMessageToTeam(teamTeamsId, serviceUrl, message, _adapter, _credentials, cancellationToken);
                                SimpleDatabase.UpdateSelfIntroConversationActivityId(teamId, userId, conversationId, activityId);
                            }, cancellationToken
                        );
                        SimpleDatabase.UpdateToDoUserValue(teamId, userId, 0, true);
                    }
                    else
                    {
                        _ = SimpleMessenger.UpdateMessage(conversationId, activityId, serviceUrl, message, _adapter, _appId, cancellationToken);
                    }
                }
            }
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
