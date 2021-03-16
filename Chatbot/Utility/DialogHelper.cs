using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Collections.Generic;
using System.Linq;

namespace Chatbot.Utility
{
    public class DialogHelper
    {
        public static AdaptiveCard CreateChoiceCard(List<Choice> choices, string message = "What do you want to do?")
        {
            var actions = choices.Select(choice => new AdaptiveSubmitAction
            {
                Title = choice.Value,
                Data = choice.Value
            }).ToList<AdaptiveAction>();

            return new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock
                        {
                            Text = message,
                            Wrap = true,
                        },
                },
                Actions = actions
            };
        }
    }
}