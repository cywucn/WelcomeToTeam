using Newtonsoft.Json;

namespace WelcomeToTeam
{
    public class RegistrationCardSubmitData
    {
        [JsonProperty(PropertyName = "teamId")]
        public string TeamId { get; set; }
    }
}
