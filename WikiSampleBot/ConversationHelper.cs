namespace WikiSampleBot
{
    using System;
    using System.Linq;
    using Microsoft.Bot.Connector;

    public class ConversationHelper
    {
        public static bool CanHandleActivity(IMessageActivity message, bool isAnswering, ChannelAccount botAccount)
        {
            // this is to avoid having the bot handling every message in Slack unless the bot is mentioned
            if (message != null && IsSlackChannel(message) && message.Conversation.IsGroup.GetValueOrDefault())
            {
                if (!isAnswering && (message.Entities == null || !IsBotMentioned(message, botAccount)))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsSlackChannel(IMessageActivity message)
        {
            return message != null && message.ChannelId.Equals("slack", StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsUserGoingToAnswerQuestion(IMessageActivity message)
        {
            return message != null && message.Text.StartsWith(Constants.WikiQuestionKey);
        }

        public static bool IsBotMentioned(IMessageActivity message, ChannelAccount botAccount)
        {
            if (message.Entities != null)
            {
                return message.Entities.Any(x => x.GetAs<Mention>()?.Mentioned?.Id == botAccount.Id) && message.Text.StartsWith($"@{botAccount.Name}");
            }

            return false;
        }
    }
}