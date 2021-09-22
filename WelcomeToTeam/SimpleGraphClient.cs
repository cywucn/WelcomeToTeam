using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Graph;
using Microsoft.Identity.Client;


namespace WelcomeToTeam
{
    public static class SimpleGraphClient
    {
        public static GraphServiceClient GetAuthenticatedClient(string token)
        {
            GraphServiceClient graphClient = new GraphServiceClient
            (
                new DelegateAuthenticationProvider
                (
                    r =>
                    {
                        r.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                        return Task.CompletedTask;
                    }
                )
            );
            return graphClient;
        }

        private static readonly string[] Scopes = new string[] { "https://graph.microsoft.com/.default" };
        public static async Task<GraphServiceClient> GetAuthenticatedClient(string appId, string appPassword, string tenantId)
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(appId).WithClientSecret(appPassword).WithAuthority($"https://login.microsoftonline.com/{tenantId}").Build();
            string accessToken = (await app.AcquireTokenForClient(Scopes).ExecuteAsync()).AccessToken;
            GraphServiceClient graphClient = new GraphServiceClient
            (
                new DelegateAuthenticationProvider
                (
                    r =>
                    {
                        r.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                        return Task.CompletedTask;
                    }
                )
            );
            return graphClient;
        }



        public static async Task<(string userName, string userPhotoUrl)> GetUserNamePhotoUrl(GraphServiceClient graphClient, string userId)
        {
            Task<User> userTask = graphClient.Users[userId].Request().Select(u => new { u.DisplayName }).GetAsync();
            string userName;
            string userPhotoUrl;
            try
            {
                Task<ProfilePhoto> userPhotoMetadataTask = graphClient.Users[userId].Photo.Request().GetAsync();
                Task<Stream> userPhotoStreamTask = graphClient.Users[userId].Photo.Content.Request().GetAsync();
                userName = (await userTask).DisplayName;
                ProfilePhoto userPhotoMetadata = await userPhotoMetadataTask;
                Stream userPhotoStream = await userPhotoStreamTask;

                string userPhotoMediaContentType = userPhotoMetadata.AdditionalData["@odata.mediaContentType"].ToString();
                byte[] userPhoto = new byte[userPhotoStream.Length];
                userPhotoStream.Read(userPhoto);
                string commentUserPhotoBase64String = Convert.ToBase64String(userPhoto);
                userPhotoUrl = $"data:{userPhotoMediaContentType};base64,{commentUserPhotoBase64String}";
            }
            catch (ServiceException)
            {
                userName = (await userTask).DisplayName;
                userPhotoUrl = $"https://ui-avatars.com/api/?name={HttpUtility.UrlEncode(userName)}";
            }
            return (userName, userPhotoUrl);
        }

        public static async Task<List<(string userName, string userPhotoUrl)>> GetUserNamePhotoUrlList(GraphServiceClient graphClient, List<string> userIdList)
        {
            List<Task<(string userName, string userPhotoUrl)>> userNamePhotoUrlTaskList = new();
            foreach (string userId in userIdList)
            {
                userNamePhotoUrlTaskList.Add(GetUserNamePhotoUrl(graphClient, userId));
            }
            List<(string userName, string userPhotoUrl)> userNamePhotoUrlList = new();
            foreach (var userNamePhotoUrlTask in userNamePhotoUrlTaskList)
            {
                userNamePhotoUrlList.Add(await userNamePhotoUrlTask);
            }
            return userNamePhotoUrlList;
        }
    }
}
