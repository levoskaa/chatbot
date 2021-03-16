// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.11.1

using Chatbot.Extensions;
using Chatbot.Models;
using Chatbot.Utility;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chatbot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ComplexParsingDialog complexDialog, SimpleParsingDialog simpleDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            Logger = logger;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(complexDialog);
            AddDialog(simpleDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
             {
                QueryParsingStepAsync,
                ShowResultStepAsync,
                FinalStepAsync
             }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> QueryParsingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = stepContext.Options as MainDialogOptions
                ?? new MainDialogOptions
                {
                    SwitchToStrict = false
                };
            if (options.SwitchToStrict)
            {
                return await stepContext.BeginDialogAsync(nameof(SimpleParsingDialog), options.Message, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(ComplexParsingDialog), options.Message, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ShowResultStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string messageText;
            Activity message;
            // If the child dialog ("ComplexParsingDialog") was cancelled or something went wrong, the Result here will be null.
            if (stepContext.Result != null /*is BookingDetails result*/)
            {
                // Now we have all the constraints, we can execute the query.
                messageText = "Here are the results of your query...";
                message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);

                var choices = new List<Choice>
                {
                    new Choice("Yes, create new query"),
                    new Choice("No, modify query"),
                    new Choice("No, switch to stricter chatbot")
                };

                var card = DialogHelper.CreateChoiceCard(choices, "Are you satisfied with the result?");
                var cardActivity = (Activity)card.CreateActivity();

                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = cardActivity,
                    RetryPrompt = cardActivity,
                    Choices = choices,
                    Style = ListStyle.None
                }, cancellationToken);
            }
            else
            {
                messageText = "Something went wrong, clearing query and starting over.";
                return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            MainDialogOptions options = new MainDialogOptions();
            var choiceResult = (stepContext.Result as FoundChoice).Value;
            switch (choiceResult)
            {
                case "Yes, create new query":
                    options.Message = "What else are you looking for?";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, options, cancellationToken);

                case "No, modify query":
                    options.Message = "You can continue editing your query.";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, options, cancellationToken);

                case "No, switch to stricter chatbot":
                    options.SwitchToStrict = true;
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, options, cancellationToken);

                default:
                    options.Message = "Something went wrong, clearing query and starting over. You have to specify the object type again!";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, options, cancellationToken);
            }
        }
    }
}