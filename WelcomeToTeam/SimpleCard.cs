using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;




namespace WelcomeToTeam
{
    public static class SimpleCard
    {
        private static readonly AdaptiveSchemaVersion _adaptiveSchemaVersion = new AdaptiveSchemaVersion(1, 4);

        public static AdaptiveCard SelfIntroCard(string teamId)
        {
            string teamName = SimpleDatabase.GetTeamName(teamId);
            AdaptiveCard adaptiveCard = new AdaptiveCard(_adaptiveSchemaVersion) { };
            adaptiveCard.Body.Add
            (
                new AdaptiveTextBlock()
                {
                    Text = $"Welcome to {teamName}! Would you like to introduce yourself to our team? Click the button below and add a self-introduction!",
                    Wrap = true
                }
            );
            AdaptiveCard subAdaptiveCard = new AdaptiveCard(_adaptiveSchemaVersion) { };
            subAdaptiveCard.Body.Add
            (
                new AdaptiveTextInput()
                {
                    Id = "submission",
                    IsRequired = true,
                    InlineAction = new AdaptiveExecuteAction()
                    {
                        Title = "Submit",
                        Verb = "submitSelfIntro",
                        Data = new SelfIntroCardSubmitData()
                        {
                            TeamId = teamId
                        }
                    }
                }
            );
            adaptiveCard.Actions.Add
            (
                new AdaptiveShowCardAction()
                {
                    Title = "Add a New Self-Introduction/Modify an Existing One",
                    Card = subAdaptiveCard
                }
            );
            return adaptiveCard;
        }

        public static AdaptiveCard SelfIntroCompleteCard(string teamId)
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(_adaptiveSchemaVersion) { };
            adaptiveCard.Body.Add
            (
                new AdaptiveTextBlock()
                {
                    Text = "Your self-introduction has been received! It is now available to the whole team. Check it out in our team's default channel!",
                    Wrap = true
                }
            );
            adaptiveCard.Actions.Add
            (
                new AdaptiveExecuteAction()
                {
                    Title = "Modify",
                    Verb = "modifySelfIntro",
                    Data = new SelfIntroCardSubmitData()
                    {
                        TeamId = teamId
                    }
                }
            );
            return adaptiveCard;
        }

        public static async Task<AdaptiveCard> CommentCard(Microsoft.Graph.GraphServiceClient graphClient, string teamId, string userId)
        {

            Task<(string userName, string userPhotoUrl)> userNamePhotoUrlTask = SimpleGraphClient.GetUserNamePhotoUrl(graphClient, userId);

            var commentList = SimpleDatabase.GetCommentList(teamId, userId);
            List<string> commentUserList = new();
            foreach ((string commentUserId, _, _) in commentList)
            {
                commentUserList.Add(commentUserId);
            }
            Task<List<(string userName, string userPhotoUrl)>> commentUserNamePhotoUrlListTask = SimpleGraphClient.GetUserNamePhotoUrlList(graphClient, commentUserList);

            (string userName, string userPhotoUrl) = await userNamePhotoUrlTask;
            List<(string userName, string userPhotoUrl)> commentUserNamePhotoUrlList = await commentUserNamePhotoUrlListTask;

            AdaptiveCard adaptiveCard = new AdaptiveCard(_adaptiveSchemaVersion) { };
            adaptiveCard.Body.Add
            (
                new AdaptiveTextBlock()
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Text = $"Hello everyone! Please join me to welcome {userName}! Below are his/her own words:",
                    Wrap = true
                }
            );
            string selfIntro = SimpleDatabase.GetSelfIntro(teamId, userId);
            {
                AdaptiveColumnSet adaptiveColumnSet = new AdaptiveColumnSet();
                AdaptiveColumn adaptiveColumn = new AdaptiveColumn()
                {
                    Width = "auto"
                };
                adaptiveColumn.Items.Add
                (
                    new AdaptiveImageWithLongUrl()
                    {
                        Size = AdaptiveImageSize.Large,
                        Style = AdaptiveImageStyle.Person,
                        LongUrl = userPhotoUrl
                    }
                );
                adaptiveColumnSet.Columns.Add
                (
                    adaptiveColumn
                );
                adaptiveColumn = new AdaptiveColumn()
                {
                    VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                    Width = "stretch"
                };
                adaptiveColumn.Items.Add
                (
                    new AdaptiveTextBlock()
                    {
                        Text = selfIntro,
                        Wrap = true
                    }
                );
                adaptiveColumnSet.Columns.Add
                (
                    adaptiveColumn
                );
                adaptiveCard.Body.Add
                (
                    adaptiveColumnSet
                );
            }
            int commentUserCount = commentList.Count;
            if (commentUserCount > 0)
            {
                adaptiveCard.Body.Add
                (
                    new AdaptiveTextBlock()
                    {
                        Spacing = AdaptiveSpacing.Large,
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = "Below are some words from our team:",
                        Wrap = true
                    }
                );
                for (int i = 0; i < commentUserCount; ++i)
                {
                    (string commentUserName, string commentUserPhotoUrl) = commentUserNamePhotoUrlList[i];
                    AdaptiveColumnSet adaptiveColumnSet = new AdaptiveColumnSet()
                    {
                        Separator = true
                    };
                    AdaptiveColumn adaptiveColumn = new AdaptiveColumn() { Width = "auto" };
                    adaptiveColumn.Items.Add
                    (
                        new AdaptiveImageWithLongUrl()
                        {
                            Size = AdaptiveImageSize.Medium,
                            Style = AdaptiveImageStyle.Person,
                            LongUrl = commentUserPhotoUrl
                        }
                    );
                    adaptiveColumnSet.Columns.Add
                    (
                        adaptiveColumn
                    );
                    adaptiveColumn = new AdaptiveColumn() { Width = "stretch" };
                    adaptiveColumn.Items.Add
                    (
                        new AdaptiveTextBlock()
                        {
                            Text = commentUserName,
                            Wrap = true
                        }
                    );
                    string dateTimeStr = (new DateTime(commentList[i].time, DateTimeKind.Utc)).ToString("s") + 'Z';
                    adaptiveColumn.Items.Add
                    (
                        new AdaptiveTextBlock()
                        {
                            Spacing = AdaptiveSpacing.None,
                            IsSubtle = true,
                            Text = $"{{{{DATE({dateTimeStr})}}}} {{{{TIME({dateTimeStr})}}}}",
                            Wrap = true
                        }
                    );
                    adaptiveColumnSet.Columns.Add
                    (
                        adaptiveColumn
                    );
                    adaptiveCard.Body.Add
                    (
                        adaptiveColumnSet
                    );
                    adaptiveCard.Body.Add
                    (
                        new AdaptiveTextBlock()
                        {
                            Text = commentList[i].comment,
                            Wrap = true
                        }
                    );
                }
            }
            adaptiveCard.Actions.Add
            (
                new AdaptiveSubmitAction()
                {
                    Title = "Give Him/Her a Welcome",
                    Data = new CommentCardSubmitData()
                    {
                        MsTeams = new CommentCardSubmitDataMsTeams()
                        {
                            Type = "task/fetch"
                        },
                        Verb = "getCommentInputTaskInfo",
                        TeamId = teamId,
                        UserId = userId
                    }
                }
            );
            return adaptiveCard;
        }

        public static TaskModuleResponse CommentInputTaskModuleResponse(string teamId, string userId)
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(_adaptiveSchemaVersion) { };
            adaptiveCard.Body.Add
            (
                new AdaptiveTextInput()
                {
                    Id = "submission",
                    IsRequired = true,
                    InlineAction = new AdaptiveSubmitAction()
                    {
                        Title = "Submit",
                        Data = new CommentCardSubmitData()
                        {
                            MsTeams = new CommentCardSubmitDataMsTeams()
                            {
                                Type = "task/submit"
                            },
                            Verb = "submitComment",
                            TeamId = teamId,
                            UserId = userId
                        }
                    }
                }
            );
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
            TaskModuleTaskInfo taskModuleTaskInfo = new TaskModuleTaskInfo()
            {
                Title = "Say Something to Him/Her...",
                Card = attachment
            };
            TaskModuleContinueResponse taskModuleContinueResponse = new TaskModuleContinueResponse()
            {
                Value = taskModuleTaskInfo
            };
            TaskModuleResponse taskModuleResponse = new TaskModuleResponse
            {
                Task = taskModuleContinueResponse
            };
            return taskModuleResponse;
        }
        public static IActivity Message(AdaptiveCard adaptiveCard)
        {
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };
            return MessageFactory.Attachment(attachment);
        }


        public static AdaptiveCard ToDoCard(string userId)
        {
            List<(string teamId, int value, string title, string content)> toDoList = SimpleDatabase.GetToDoList(userId);
            List<(string teamId, int value)> toDoUserValueList = SimpleDatabase.GetToDoUserValueList(userId);
            HashSet<(string teamId, int value)> toDoUserValueSet = new(toDoUserValueList);

            AdaptiveCard adaptiveCard = new AdaptiveCard(_adaptiveSchemaVersion) { };
            string curTeamId = null;
            int curTeamIndex = 1;
            foreach ((string teamId, int value, string title, _) in toDoList)
            {
                if (curTeamId != teamId)
                {
                    curTeamId = teamId;
                    string teamName = SimpleDatabase.GetTeamName(teamId);
                    curTeamIndex = 1;
                    adaptiveCard.Body.Add(new AdaptiveTextBlock()
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = teamName,
                        Wrap = true
                    });
                }
                bool flag = toDoUserValueSet.Contains((teamId, value));
                AdaptiveColumnSet adaptiveColumnSet = new()
                {
                    Columns = new()
                    {
                        new AdaptiveColumn()
                        {
                            Width = "auto",
                            Items = new()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text = $"{curTeamIndex}."
                                }
                            }
                        }
                        ,
                        new AdaptiveColumn()
                        {
                            Width = "stretch",
                            Items = new()
                            {
                                new AdaptiveRichTextBlock()
                                {
                                    Inlines = new List<AdaptiveInline> { new AdaptiveTextRun() { Italic = flag, Strikethrough = flag, Text = title } },
                                }
                            }
                        }
                    }
                };
                adaptiveCard.Body.Add(adaptiveColumnSet);
                ++curTeamIndex;
            }
            return adaptiveCard;
        }

        public static AdaptiveCard RegistrationCard(string teamId, bool registered)
        {
            AdaptiveCard adaptiveCard = new AdaptiveCard(_adaptiveSchemaVersion) { };
            adaptiveCard.Body.Add
                (
                    new AdaptiveTextBlock()
                    {
                        Text = registered ? "Thank you." : "Click the button below to register yourself in app.",
                        Wrap = true
                    }
                );
            adaptiveCard.Actions.Add
            (
                new AdaptiveExecuteAction()
                {
                    Title = "Register",
                    Verb = "register",
                    Data = new RegistrationCardSubmitData()
                    {
                        TeamId = teamId
                    }
                }
            );
            return adaptiveCard;
        }
    }
}
