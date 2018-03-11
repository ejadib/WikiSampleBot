namespace WikiSampleBot.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class KnowledgeBasePayload
    {
        [JsonProperty(PropertyName = "qnaList")]
        public IEnumerable<QuestionAndAnswer> QuestionsAndAnswers { get; set; }
    }
}