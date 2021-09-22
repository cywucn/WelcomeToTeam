using Newtonsoft.Json;

namespace WelcomeToTeam
{
    public class CommentCardSubmitDataMsTeams
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }


    }
    public class CommentCardSubmitData
    {

        [JsonProperty(PropertyName = "msteams", NullValueHandling = NullValueHandling.Ignore)]
        public CommentCardSubmitDataMsTeams MsTeams;

        [JsonProperty(PropertyName = "verb")]
        public string Verb { get; set; }
        [JsonProperty(PropertyName = "teamId", NullValueHandling = NullValueHandling.Ignore)]
        public string TeamId { get; set; }
        [JsonProperty(PropertyName = "userId", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }
        [JsonProperty(PropertyName = "submission", NullValueHandling = NullValueHandling.Ignore)]
        public string Submission { get; set; }
    }
}
