namespace WikiSampleBot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Newtonsoft.Json;
    using WikiSampleBot.Models;

    [Serializable]
    public class UpdateQnAKnowledgeBaseService
    {
        private const string QnaMakerBaseUri = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/{0}";
     
        private readonly string qnaMakerSubscriptionKey;
        private readonly Uri updateKnowledgeUri;
        private readonly Uri publishKnowledgeUri;

        public UpdateQnAKnowledgeBaseService(string qnaMakerSubscriptionKey, string qnaMakerKnowledgeBaseId)
        {
            SetField.NotNull(out this.qnaMakerSubscriptionKey, nameof(qnaMakerSubscriptionKey), qnaMakerSubscriptionKey);
            SetField.CheckNull(nameof(qnaMakerKnowledgeBaseId), qnaMakerKnowledgeBaseId);

            this.updateKnowledgeUri = new Uri(string.Format(QnaMakerBaseUri, qnaMakerKnowledgeBaseId));
            this.publishKnowledgeUri = new Uri(string.Format(QnaMakerBaseUri, qnaMakerKnowledgeBaseId));
        }

        public async Task AddAnswer(string question, string answer)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.qnaMakerSubscriptionKey);

                var body = new UpdateKnowledgeBaseBody()
                {
                    QuestionsToAdd = new QuestionsToAdd
                    {
                        QuestionsAndAnswers = new List<QuestionAndAnswer>
                        {
                            new QuestionAndAnswer
                            {
                                Question = question,
                                Answer = answer
                            }
                        }
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

                var result = await PatchAsync(client, this.updateKnowledgeUri, content);

                result.EnsureSuccessStatusCode();

                var publishResponse = await client.PutAsync(this.publishKnowledgeUri, null);

                publishResponse.EnsureSuccessStatusCode();
            }
        }

        private static async Task<HttpResponseMessage> PatchAsync(HttpClient client, Uri requestUri, HttpContent httpContent)
        {
            var method = new HttpMethod("PATCH");

            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = httpContent,
            };

            return await client.SendAsync(request);
        }
    }
}