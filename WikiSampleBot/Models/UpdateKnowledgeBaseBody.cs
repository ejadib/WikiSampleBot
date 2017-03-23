namespace WikiSampleBot.Models
{
    using Newtonsoft.Json;

    public class UpdateKnowledgeBaseBody
    {
        public UpdateKnowledgeBaseBody()
        {
        }

        [JsonProperty(PropertyName = "add")]
        public QuestionsToAdd QuestionsToAdd { get; set; }
    }
}