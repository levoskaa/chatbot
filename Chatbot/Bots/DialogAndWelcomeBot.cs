// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.11.1

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chatbot.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var messageText = "Hello!" + Environment.NewLine
                        + "I can help you look up various things. You can explicitly tell me what kind of object you are looking for or include the type in your first sentence. You could say \"I am looking for a book\" or start with a constraint, such as \"The author of the book is William Shakespeare\"." + Environment.NewLine
                        + "Every constraint will narrow down the result as the query will be executed with all of them joined together by logical AND. I check for the inclusion of values, so it doesn't have to be an exact match." + Environment.NewLine
                        + "Additionally you can list, edit or delete constraints anytime you want by stating your intent to do so." + Environment.NewLine
                        + "You have the option to restart building the query, thus deleting every constraint." + Environment.NewLine
                        + "Whenever you get stuck, try asking for help!";
                    var response = MessageFactory.Text(messageText);
                    await turnContext.SendActivityAsync(response, cancellationToken);
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
        }
    }
}