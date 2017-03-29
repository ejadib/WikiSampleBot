namespace WikiSampleBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Connector;
    using WikiSampleBot.Models;
    using WikiSampleBot.Services;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private UpdateQnAKnowledgeBaseService updateKnowledgeBaseService;

        public RootDialog(UpdateQnAKnowledgeBaseService updateKnowledgeBaseService)
        {
            SetField.NotNull(out this.updateKnowledgeBaseService, nameof(updateKnowledgeBaseService), updateKnowledgeBaseService);
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            context.PrivateConversationData.TryGetValue<bool>(Constants.IsAnsweringKey, out bool isAnswering);

            if (isAnswering)
            {
                var question = context.PrivateConversationData.Get<string>(Constants.QuestionToAnswerKey);

                var typing = context.MakeMessage();
                typing.Type = ActivityTypes.Typing;
                await context.PostAsync(typing);

                await this.updateKnowledgeBaseService.AddAnswer(question, message.Text);

                context.PrivateConversationData.RemoveValue(Constants.IsAnsweringKey);
                context.PrivateConversationData.RemoveValue(Constants.QuestionToAnswerKey);

                if (ConversationHelper.IsSlackChannel(message))
                {
                    var responseUrl = context.PrivateConversationData.Get<string>(Constants.SlackResponseUrlKey);

                    context.PrivateConversationData.RemoveValue(Constants.SlackResponseUrlKey);

                    var interactiveMessage = new SlackInteractiveMessage
                    {
                        ResponseType = "in_channel",
                        DeleteOriginal = true,
                        Text = $"Wow! I just learnt something new. Thanks {message.From.Name}!",
                        Attachments =
                            {
                                new SlackAttachment
                                {
                                    Color = "#36a64f",
                                    Title = $"{question}",
                                    Text = message.Text
                                }
                            }
                    };

                    await SendSlackInteractiveMessage(responseUrl, interactiveMessage);
                }
                else
                {
                    var reply = context.MakeMessage();
                    reply.Text = $"Wow! I just learnt something new. Thanks {message.From.Name}!";
                    reply.Attachments = new List<Attachment>
                    {
                        new HeroCard
                        {
                            Title = question,
                            Text = message.Text
                        }.ToAttachment()
                    };

                    await context.PostAsync(reply);
                }

                await context.FlushAsync(CancellationToken.None);
                context.Wait(this.MessageReceivedAsync);
            }
            else if ((ConversationHelper.IsSlackChannel(message) && (message.ChannelData == null || message.ChannelData.Payload == null)) || !ConversationHelper.IsUserGoingToAnswerQuestion(message))
            {
                // in reality we can just check if user is going to answer a question and remove the Slack specific check
                var askForHelpReply = context.MakeMessage();
                askForHelpReply.Text = $"Oops. I dont know how to answer that. Anyone willing to help?\n\n**{message.Text}**";
                askForHelpReply.Attachments = new List<Attachment>
                    {
                        new HeroCard
                        {
                            Buttons = new List<CardAction>
                            {
                                // sending the qustion as part of the button. Not required for Slack but a quick workaround for other channels.
                                new CardAction(ActionTypes.PostBack, "Answer", value: $"{Constants.WikiQuestionKey}{message.Text}")
                            }
                        }.ToAttachment()
                    };

                await context.PostAsync(askForHelpReply);

                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                string user = message.From.Name;

                // the value of the button should containt the question
                string question = message.Text.Substring(Constants.WikiQuestionKey.Length);

                context.PrivateConversationData.SetValue(Constants.IsAnsweringKey, true);
                context.PrivateConversationData.SetValue(Constants.QuestionToAnswerKey, question);

                if (ConversationHelper.IsSlackChannel(message))
                {
                    string responseUrl = message.ChannelData.Payload.response_url;

                    context.PrivateConversationData.SetValue(Constants.SlackResponseUrlKey, responseUrl);
                    await context.FlushAsync(CancellationToken.None);

                    // just showing how to access the data from the Slack payload
                    user = message.ChannelData.Payload.user.name;

                    // question = message.ChannelData.Payload.original_message.text.ToString().Replace("*", string.Empty).Replace("\n", string.Empty);
                    var interactiveMessage = new SlackInteractiveMessage
                    {
                        ResponseType = "in_channel",
                        ReplaceOriginal = true,
                        Text = "Oops. I dont know how to answer that. Anyone willing to help?",
                        Attachments =
                        {
                            new SlackAttachment
                            {
                                Color = "#36a64f",
                                Title = question,
                                Text = $"@{user} is answering"
                            }
                        }
                    };

                    await SendSlackInteractiveMessage(responseUrl, interactiveMessage);
                }
                else
                {
                    await context.FlushAsync(CancellationToken.None);
                    await context.PostAsync($"@{user} is answering");
                }

                context.Wait(this.MessageReceivedAsync);
            }
        }

        // TODO: Move to another class
        private static async Task SendSlackInteractiveMessage(string responseUrl, SlackInteractiveMessage interactiveMessage)
        {
            using (HttpClient client = new HttpClient())
            {
                await client.PostAsJsonAsync(responseUrl, interactiveMessage);
            }
        }
    }
}