using AdaptiveCards;
using Newtonsoft.Json;

namespace WelcomeToTeam
{
    public class AdaptiveImageWithLongUrl : AdaptiveImage
    {
        [JsonProperty(PropertyName = "url", Required = Required.Always)]
        public string LongUrl { get; set; }
    }
}
