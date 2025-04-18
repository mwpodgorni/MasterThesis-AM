using System.Collections.Generic;
using Newtonsoft.Json;



namespace TutorialData.Model
{
    public record TutorialData
    {
        [JsonProperty("items")]
        public List<TutorialStep> Items;

    }
    public record TutorialStep
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("content")]
        public List<string> Content { get; set; }

        [JsonProperty("event")]
        public string EventName { get; set; }
    }
    public class HelpText
    {
        public string title { get; set; }
        public List<string> description { get; set; }
    }
}