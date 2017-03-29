namespace WikiSampleBot.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class SlackInteractiveMessage
    {
        public SlackInteractiveMessage()
        {
            this.Attachments = new List<SlackAttachment>();
        }

        [JsonProperty("response_type")]
        public string ResponseType { get; set; }

        [JsonProperty("replace_original")]
        public bool ReplaceOriginal { get; set; }

        [JsonProperty("delete_original")]
        public bool DeleteOriginal { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("attachments")]
        public IList<SlackAttachment> Attachments { get; set; }
    }
}