namespace WikiSampleBot.Models
{
    using Newtonsoft.Json;

    public class SlackAttachment
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }
}