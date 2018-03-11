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
    public class WikiQnAKnowledgeBaseService
    {
        private const string QnaMakerBaseUri = "https://westus.api.cognitive.microsoft.com/qnamaker/v3.0/knowledgebases/{0}";
     
        private readonly string qnaMakerSubscriptionKey;
        private readonly Uri qnaMakerUri;

        public WikiQnAKnowledgeBaseService(string qnaMakerSubscriptionKey, string qnaMakerKnowledgeBaseId)
        {
            SetField.NotNull(out this.qnaMakerSubscriptionKey, nameof(qnaMakerSubscriptionKey), qnaMakerSubscriptionKey);
            SetField.CheckNull(nameof(qnaMakerKnowledgeBaseId), qnaMakerKnowledgeBaseId);

            this.qnaMakerUri = new Uri(string.Format(QnaMakerBaseUri, qnaMakerKnowledgeBaseId));
        }

        public async Task AddAnswer(string question, string answer)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.qnaMakerSubscriptionKey);

                var body = new UpdateKnowledgeBaseBody()
                {
                    QuestionsToAdd = new KnowledgeBasePayload
                    {
                        QuestionsAndAnswers = new List<QuestionAndAnswer>
                        {
                            new QuestionAndAnswer
                            {
                                Questions = new[] { question },
                                Answer = answer
                            }
                        }
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

                var result = await PatchAsync(client, this.qnaMakerUri, content);

                result.EnsureSuccessStatusCode();

                var publishResponse = await client.PutAsync(this.qnaMakerUri, null);

                publishResponse.EnsureSuccessStatusCode();
            }
        }

        public async Task<KnowledgeBasePayload> DownloadKb()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.qnaMakerSubscriptionKey);

                var downloadKbResponse = await client.GetAsync(this.qnaMakerUri).ConfigureAwait(false);

                downloadKbResponse.EnsureSuccessStatusCode();

                var kb = await downloadKbResponse.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<KnowledgeBasePayload>(kb);
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