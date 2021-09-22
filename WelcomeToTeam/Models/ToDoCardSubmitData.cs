using Newtonsoft.Json;

namespace WelcomeToTeam
{
    public class ToDoCardSubmitData
    {
        [JsonProperty(PropertyName = "toDoValue", NullValueHandling = NullValueHandling.Ignore)]
        public string ToDoValue { get; set; }
    }
}
