using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;



namespace WelcomeToTeam
{
    public class WelcomeToTeamBot : TeamsActivityHandler
    {
        private readonly string _appId;
        private readonly string _appPassword;
        private readonly MicrosoftAppCredentials _credentials;

        private readonly string _clientId;
        private readonly string _clientSecret;

        private readonly BotFrameworkHttpAdapter _adapter;

        private readonly ConversationState _conversationState;
        private readonly MainDialog _dialog;

        public WelcomeToTeamBot(IConfiguration configuration, BotFrameworkHttpAdapter adapter, ConversationState conversationState, MainDialog dialog)
        {
            _appId = configuration["MicrosoftAppId"];
            _appPassword = configuration["MicrosoftAppPassword"];
            _credentials = new MicrosoftAppCredentials(_appId, _appPassword);

            _clientId = configuration["CLIENT_ID"];
            _clientSecret = configuration["CLIENT_SECRET"];

            _adapter = adapter;

            _conversationState = conversationState;
            _dialog = dialog;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnSignInInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
        protected override async Task OnTokenResponseEventAsync(ITurnContext<IEventActivity> turnContext, CancellationToken cancellationToken)
        {
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }
        private static void OnBotAdded(string teamId, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            string teamTeamsId = teamInfo.Id;
            string teamName = teamInfo.Name;
            SimpleDatabase.UpdateTeamInfo(teamId, teamTeamsId, teamName);
            SimpleDatabase.UpdateToDoList(teamId, 0, "Self-Introduction", "Submit a self-introduction.");
            IActivity message = SimpleCard.Message(SimpleCard.RegistrationCard(teamId, false));
            _ = turnContext.SendActivityAsync(message, cancellationToken);
        }


        protected override Task OnTeamsMembersAddedAsync(IList<TeamsChannelAccount> teamsMembersAdded, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (teamInfo is null)
            {
                return Task.CompletedTask;
            }
            if (turnContext.Activity.From.AadObjectId is null)
            {
                return Task.CompletedTask;
            }
            string teamId = teamInfo.AadGroupId;
            string botTeamsId = turnContext.Activity.Recipient.Id;
            string tenantId = turnContext.Activity.Conversation.TenantId;
            string serviceUrl = turnContext.Activity.ServiceUrl;
            foreach (var teamMemberAdded in teamsMembersAdded)
            {
                string userTeamsId = teamMemberAdded.Id;
                if (botTeamsId == userTeamsId)
                {
                    OnBotAdded(teamId, teamInfo, turnContext, cancellationToken);
                }
                else
                {
                    string userId = teamMemberAdded.AadObjectId;
                    if (!SimpleDatabase.IsUserInTeam(teamId, userId))
                    {
                        SimpleDatabase.UpdateTeamUser(teamId, userId, false);
                        IActivity message = SimpleCard.Message(SimpleCard.SelfIntroCard(teamId));
                        _ = SimpleMessenger.SendMessageToMember(userTeamsId, botTeamsId, tenantId, serviceUrl, message, _adapter, _credentials, cancellationToken);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private AdaptiveCard OnSubmitSelfIntro(ITurnContext<IInvokeActivity> turnContext, object data, CancellationToken cancellationToken)
        {
            SelfIntroCardSubmitData selfIntroCardSubmitData = JsonConvert.DeserializeObject<SelfIntroCardSubmitData>(data.ToString());
            string userId = turnContext.Activity.From.AadObjectId;
            string teamId = selfIntroCardSubmitData.TeamId;
            SimpleDatabase.UpdateSelfIntro(teamId, userId, selfIntroCardSubmitData.Submission);

            string serviceUrl = turnContext.Activity.ServiceUrl;
            (string ssoConversationId, string ssoUserTeamsId) = SimpleDatabase.GetSSOConversationIdUserTeamsId();
            if (ssoConversationId is not null)
            {
                _ = SimpleMessenger.StartDialog(teamId, userId, ssoConversationId, ssoUserTeamsId, serviceUrl, _adapter, _conversationState, _dialog, _appId, cancellationToken);
            }

            return SimpleCard.SelfIntroCompleteCard(teamId);
        }

        private static AdaptiveCard OnModifySelfIntro(object data)
        {
            SelfIntroCardSubmitData selfIntroCardSubmitData = JsonConvert.DeserializeObject<SelfIntroCardSubmitData>(data.ToString());
            string teamId = selfIntroCardSubmitData.TeamId;
            return SimpleCard.SelfIntroCard(teamId);
        }

        private static AdaptiveCard OnRegister(object data, string userId)
        {
            RegistrationCardSubmitData registrationCardSubmitData = JsonConvert.DeserializeObject<RegistrationCardSubmitData>(data.ToString());
            string teamId = registrationCardSubmitData.TeamId;
            SimpleDatabase.UpdateTeamUser(teamId, userId, true);
            AdaptiveCard adaptiveCard = SimpleCard.RegistrationCard(teamId, true);
            return adaptiveCard;
        }

        protected override Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            AdaptiveCard adaptiveCard = null;
            AdaptiveCardInvokeAction adaptiveCardInvokeAction = invokeValue.Action;
            string verb = adaptiveCardInvokeAction.Verb;
            object data = adaptiveCardInvokeAction.Data;
            if (verb == "submitSelfIntro")
            {
                adaptiveCard = OnSubmitSelfIntro(turnContext, data, cancellationToken);
            }
            else if (verb == "modifySelfIntro")
            {
                adaptiveCard = OnModifySelfIntro(data);
            }
            else if (verb == "register")
            {
                string userId = turnContext.Activity.From.AadObjectId;
                adaptiveCard = OnRegister(data, userId);
            }

            AdaptiveCardInvokeResponse adaptiveCardInvokeResponse = new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = adaptiveCard
            };
            return Task.FromResult(adaptiveCardInvokeResponse);
        }
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            CommentCardSubmitData commentCardSubmitData = JsonConvert.DeserializeObject<CommentCardSubmitData>(taskModuleRequest.Data.ToString());
            string teamId = commentCardSubmitData.TeamId;
            string userId = commentCardSubmitData.UserId;
            return Task.FromResult(SimpleCard.CommentInputTaskModuleResponse(teamId, userId));
        }
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            CommentCardSubmitData commentCardSubmitData = JsonConvert.DeserializeObject<CommentCardSubmitData>(taskModuleRequest.Data.ToString());
            string teamId = commentCardSubmitData.TeamId;
            string userId = commentCardSubmitData.UserId;
            string commentUserId = turnContext.Activity.From.AadObjectId;
            SimpleDatabase.UpdateComment(teamId, userId, commentUserId, commentCardSubmitData.Submission);

            string serviceUrl = turnContext.Activity.ServiceUrl;
            (string ssoConversationId, string ssoUserTeamsId) = SimpleDatabase.GetSSOConversationIdUserTeamsId();
            if (ssoConversationId is not null)
            {
                _ = SimpleMessenger.StartDialog(teamId, userId, ssoConversationId, ssoUserTeamsId, serviceUrl, _adapter, _conversationState, _dialog, _appId, cancellationToken);
            }

            return Task.FromResult<TaskModuleResponse>(null);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Conversation.IsGroup != true)
            {
                if (turnContext.Activity.Text == "to-do")
                {
                    string userId = turnContext.Activity.From.AadObjectId;
                    await turnContext.SendActivityAsync(SimpleCard.Message(SimpleCard.ToDoCard(userId)), cancellationToken);
                }
                else if (turnContext.Activity.Text == "SSO")
                {
                    await _conversationState.ClearStateAsync(turnContext, cancellationToken);
                    await _conversationState.CreateProperty<string>("teamId").SetAsync(turnContext, null, cancellationToken);
                    await _conversationState.CreateProperty<string>("userId").SetAsync(turnContext, null, cancellationToken);
                    await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync("Send me **to-do** to view your to-dos.", cancellationToken: cancellationToken);
                }
            }
        }

        protected override Task OnTeamsMembersRemovedAsync(IList<TeamsChannelAccount> teamsMembersRemoved, TeamInfo teamInfo, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            if (teamInfo is null)
            {
                return Task.CompletedTask;
            }
            if (turnContext.Activity.From.AadObjectId is null)
            {
                return Task.CompletedTask;
            }
            string teamId = teamInfo.AadGroupId;
            string botTeamsId = turnContext.Activity.Recipient.Id;
            foreach (var teamsMemberRemoved in teamsMembersRemoved)
            {
                string userTeamsId = teamsMemberRemoved.Id;
                if (botTeamsId == userTeamsId)
                {
                    SimpleDatabase.DeleteTeam(teamId);
                }
                else
                {
                    string userId = teamsMemberRemoved.AadObjectId;
                    SimpleDatabase.DeleteUserFromTeam(teamId, userId);
                }
            }
            return Task.CompletedTask;
        }
    }
}
