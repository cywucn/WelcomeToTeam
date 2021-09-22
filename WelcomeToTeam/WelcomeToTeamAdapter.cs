using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WelcomeToTeam
{
    public class WelcomeToTeamAdapter : BotFrameworkHttpAdapter
    {
        public WelcomeToTeamAdapter(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger) : base(configuration, logger) { }
    }
}
