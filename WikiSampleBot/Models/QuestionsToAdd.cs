namespace WikiSampleBot.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class QuestionsToAdd
    {
        [JsonProperty(PropertyName = "qnaPairs")]
        public IEnumerable<QuestionAndAnswer> QuestionsAndAnswers { get; set; }
    }
}