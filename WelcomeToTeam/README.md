## WelcomeToTeam

### Overview

This is the source code of a Teams app that can welcome a new member of a team by sending welcoming messages and customizable to-do list to him/her.

### Deliverables

1.    Automated welcome message to a newcomer of a team.
2.    Adaptive card for a newcomer to write his/her self-introduction. When a newcomer clicks the button “Create”, an adaptive card will pop up to let him/her fill in the blanks. When he/she finishes editing, the self-intro will be sent automatically using an adaptive card by the bot to the default channel of his/her team. 
3.    His/her teammates can welcome him/her by clicking the button on the self-intro card and fill in the blanks of the newly emerged task module. After editing, the welcoming message will be appended to the self-intro card.
4.    Tab that contains previous self-introduction written by the newcomer’s teammates.
5.    Tab for newcomers to view their to-dos and check off items in the to-do list. Bot that can display to-dos for the newcomer.

### Code Structure

The code is written in C#. You may need to use Visual Studio 2019 to build it.

### How to Run

Since the app uses a lot of functionalities, such as SSO for Tab and Bot, Resource Specific Consent (RSC) (optional), etc, you may need to follow the document: [Microsoft Teams Platform developer documentation - Teams | Microsoft Docs](https://docs.microsoft.com/en-us/microsoftteams/platform/) to know how to setup an app registration and further configure it to use SSO and RSC (optional).

#### SSO

SSO is used for retrieving the user's profile photo.

Follow the instructions here: [Single sign-on support for tabs - Teams | Microsoft Docs](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-aad-sso) to add SSO support for Tab.

Follow the instructions here: [Single sign-on support for bots - Teams | Microsoft Docs](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots) to add SSO support for Bot.

Moreover, the app requires the Microsoft Graph delegated permission: `User.ReadBasic.All`. You should consent this delegated permission in the app's registration page.

#### RSC (optional)

Follow the instructions here: [Enable resource-specific consent in Teams - Teams | Microsoft Docs](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/rsc/resource-specific-consent) to enable RSC, which is used for retrieving the members of a team. The RSC permission that the app use is `TeamMember.Read.Group`. You should add this permission to the app's manifest.

### Example Manifest

```json
{
    "$schema": "https://developer.microsoft.com/en-us/json-schemas/teams/v1.9/MicrosoftTeams.schema.json",
    "manifestVersion": "1.9",
    "version": "1.0.0",
    "id": "eddf3d6f-1a45-40a9-aa26-9a90eb3f5479",
    "packageName": "com.company.example",
    "developer": {
        "name": "My Company",
        "websiteUrl": "https://cywu.azurewebsites.net",
        "privacyUrl": "https://cywu.azurewebsites.net",
        "termsOfUseUrl": "https://cywu.azurewebsites.net"
    },
    "icons": {
        "color": "color.png",
        "outline": "outline.png"
    },
    "name": {
        "short": "WelcomeToTeam",
        "full": "WelcomeToTeam"
    },
    "description": {
        "short": "Short description for WelcomeToTeam.",
        "full": "Full description of WelcomeToTeam."
    },
    "accentColor": "#FFFFFF",
    "staticTabs": [
        {
            "entityId": "conversations",
            "scopes": [
                "personal"
            ]
        },
        {
            "entityId": "toDo",
            "name": "To-Do",
            "contentUrl": "https://cywu.azurewebsites.net/todo",
            "scopes": [
                "personal"
            ]
        },
        {
            "entityId": "selfIntro",
            "name": "Teammates",
            "contentUrl": "https://cywu.azurewebsites.net/selfintro",
            "scopes": [
                "personal"
            ]
        },
        {
            "entityId": "about",
            "scopes": [
                "personal"
            ]
        }
    ],
    "bots": [
        {
            "botId": "47e99a16-f8a5-4ed9-8d3b-afeb3af0d786",
            "scopes": [
                "personal",
                "team"
            ],
            "supportsFiles": false,
            "isNotificationOnly": false
        }
    ],
    "permissions": [
        "identity",
        "messageTeamMembers"
    ],
    "validDomains": [
        "cywu.azurewebsites.net",
        "token.botframework.com"
    ],
    "webApplicationInfo": {
        "id": "fc42f8f5-8bc2-488b-b1ec-4da78ab70fb9",
        "resource": "api://cywu.azurewebsites.net/fc42f8f5-8bc2-488b-b1ec-4da78ab70fb9/botid-47e99a16-f8a5-4ed9-8d3b-afeb3af0d786"
    }
}
```
