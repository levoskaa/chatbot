using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Chatbot.Extensions
{
    public static class AdaptiveCardExtensions
    {
        public static IMessageActivity CreateActivity(this AdaptiveCard card)
        {
            return MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JObject.FromObject(card),
            });
        }
    }
}