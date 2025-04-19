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
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public List<string> Description { get; set; }

        // simple image path (no extra struct)
        [JsonProperty("visual")]
        public string Visual { get; set; }


        [JsonProperty("highlights")]
        public List<string> Highlights { get; set; }

    }


}