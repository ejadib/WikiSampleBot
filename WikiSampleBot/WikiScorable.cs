namespace WikiSampleBot
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;

    public class WikiScorable : IScorable<IActivity, double>
    {
        private IScorable<IActivity, double> inner;
        private IDialogStack stack;
        private IBotData botData;

        public WikiScorable(QnAMakerScorable inner, IDialogStack stack, IBotData botData)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.botData, nameof(botData), botData);
        }

        public async Task<object> PrepareAsync(IActivity item, CancellationToken token)
        {
            this.botData.PrivateConversationData.TryGetValue<bool>(Constants.IsAnsweringKey, out bool isAnswering);

            var messageActivity = item.AsMessageActivity();

            var botAccount = messageActivity?.Recipient;

            if (ConversationHelper.IsUserGoingToAnswerQuestion(messageActivity) || isAnswering)
            {
                return null;
            }

            if (!ConversationHelper.CanHandleActivity(messageActivity, isAnswering, botAccount))
            {
                return "reset";
            }

            if (messageActivity != null && ConversationHelper.IsBotMentioned(messageActivity, botAccount))
            {
                // removes any mention from the text
                messageActivity.Text = messageActivity.Text.Replace("@" + messageActivity.Recipient.Name, string.Empty).Trim();
            }

            return await this.inner.PrepareAsync(item, token);
        }

        public bool HasScore(IActivity item, object state)
        {
            if (state != null && state.Equals("reset"))
            {
                return true;
            }

            return this.inner.HasScore(item, state);
        }

        public double GetScore(IActivity item, object state)
        {
            if (state != null && state.Equals("reset"))
            {
                return 1.0;
            }

            return this.inner.GetScore(item, state);
        }

        public async Task PostAsync(IActivity item, object state, CancellationToken token)
        {
            if (state != null && state.Equals("reset"))
            {
                this.stack.Reset();
            }
            else
            {
                await this.inner.PostAsync(item, state, token);
            }
        }

        public Task DoneAsync(IActivity item, object state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}