using Newtonsoft.Json;

namespace WelcomeToTeam
{
    public class ToDoItem
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
    }
}
