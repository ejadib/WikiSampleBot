namespace WikiSampleBot.Models
{
    using Newtonsoft.Json;

    public class QuestionAndAnswer
    {
        [JsonProperty(PropertyName = "question")]
        public string Question { get; set; }

        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }
    }
}
