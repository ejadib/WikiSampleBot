namespace WikiSampleBot.Models
{
    using Newtonsoft.Json;

    public class QuestionAndAnswer
    {
        [JsonProperty(PropertyName = "questions")]
        public string[] Questions { get; set; }

        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }
    }
}
