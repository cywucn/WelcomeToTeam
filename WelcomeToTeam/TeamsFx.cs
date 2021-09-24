using System;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.JSInterop;

namespace WelcomeToTeam
{
    public class TeamsFx
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IJSObjectReference _module;

        public static async Task<TeamsFx> CreateTeamsFx(IJSRuntime jsRuntime)
        {
            var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./teamsfx.js");
            return new TeamsFx(jsRuntime, module);
        }

        public TeamsFx(IJSRuntime jsRuntime, IJSObjectReference module)
        {
            this._jsRuntime = jsRuntime;
            this._module = module;
        }

        public async Task Init(string clientid, string authEndpoint, string tabEndpoint)
        {
            await _module.InvokeVoidAsync("initializeTeamsSdk", clientid, authEndpoint, tabEndpoint);
        }
        public class UserInfo
        {
            public string DisplayName { get; set; }
            public string PreferredUserName { get; set; }
            public string ObjectId { get; set; }
        }
        public async Task<UserInfo> GetInfoAsync()
        {
            var user = await _module.InvokeAsync<UserInfo>("getUserInfo");
            return user;
        }

        public async Task<GraphServiceClient> GetGraphServiceClient()
        {
            var accessToken = await GetAuthenticationToken();
            if (accessToken is null)
            {
                return null;
            }
            var client = new GraphServiceClient
            (
                new DelegateAuthenticationProvider
                (
                    (r) =>
                    {
                        r.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Token);
                        return Task.CompletedTask;
                    }
                )
            );
            return client;
        }

        public class AccessToken
        {
            public string Token { get; set; }
            public string ExpiresOnTimestamp { get; set; }
        }

        public async Task<AccessToken> GetAuthenticationToken()
        {
            AccessToken token;
            try
            {
                token = await _module.InvokeAsync<AccessToken>("getToken");
            }
            catch (Exception)
            {
                await _module.InvokeVoidAsync("popupLoginPage");
                return null;
            }
            return token;
        }

        public async Task LoadTabConfigJS()
        {
            await _jsRuntime.InvokeVoidAsync("eval", "document.getElementById('tab config').appendChild(Object.assign(document.createElement('script'),{src: './tabconfig.js' })); ");
        }
    }
}
