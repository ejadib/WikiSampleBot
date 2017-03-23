namespace WikiSampleBot.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class SlackInteractiveMessages
    {
        public SlackInteractiveMessages()
        {
            this.Attachments = new List<SlackAttachment>();
        }

        [JsonProperty("response_type")]
        public string ResponseType { get; set; }

        [JsonProperty("replace_original")]
        public bool ReplaceOriginal { get; set; }

        [JsonProperty("attachments")]
        public IList<SlackAttachment> Attachments { get; set; }
    }
}