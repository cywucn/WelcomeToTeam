using Newtonsoft.Json;

namespace WelcomeToTeam
{
    public class SelfIntroCardSubmitData
    {

        [JsonProperty(PropertyName = "teamId")]
        public string TeamId { get; set; }
        [JsonProperty(PropertyName = "submission", NullValueHandling = NullValueHandling.Ignore)]
        public string Submission { get; set; }
    }
}
