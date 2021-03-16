using AdaptiveCards;
using Chatbot.CognitiveModels;
using Chatbot.Extensions;
using Chatbot.Recognizers;
using Chatbot.Utility;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chatbot.Dialogs
{
    public class ComplexParsingDialog : CancelAndHelpDialog
    {
        protected readonly ILogger logger;
        private readonly ComplexStatementRecognizer complexRecognizer;
        private readonly SimpleStatementRecognizer simpleRecognizer;
        private string currentIntent;

        public ComplexParsingDialog(ComplexStatementRecognizer complexRecognizer, SimpleStatementRecognizer simpleRecognizer, ILogger<ComplexParsingDialog> logger)
            : base(nameof(ComplexParsingDialog))
        {
            this.complexRecognizer = complexRecognizer;
            this.simpleRecognizer = simpleRecognizer;
            this.logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PromptStepAsync,
                ActStepAsync,
                ProcessDonePromptAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = stepContext.Options?.ToString() ?? "What are you looking for?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var complexResult = await complexRecognizer.RecognizeAsync<ComplexModel>(stepContext.Context, cancellationToken);
            var topIntent = complexResult.TopIntent().intent;
            currentIntent = topIntent.ToString();

            switch (topIntent)
            {
                case ComplexModel.Intent.Statement:
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, "Statement intent recognized", cancellationToken);

                case ComplexModel.Intent.ObjectType:
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, "Object type intent recognized", cancellationToken);

                case ComplexModel.Intent.Done:
                    var choices = new List<Choice>
                    {
                      new Choice("Show result"),
                      new Choice("Return to editing query")
                    };

                    var card = DialogHelper.CreateChoiceCard(choices);
                    var cardActivity = (Activity)card.CreateActivity();

                    return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                    {
                        Prompt = cardActivity,
                        RetryPrompt = cardActivity,
                        Choices = choices,
                        Style = ListStyle.None
                    }, cancellationToken);

                case ComplexModel.Intent.Delete:
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, "Delete intent recognized", cancellationToken);

                case ComplexModel.Intent.Edit:
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, "Edit intent recognized", cancellationToken);

                case ComplexModel.Intent.Exit:
                    return await stepContext.EndDialogAsync(null, cancellationToken);

                case ComplexModel.Intent.Help:
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, "Help intent recognized", cancellationToken);

                default:
                    var noIntentMessage = "Sorry, I could not understand that.";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, noIntentMessage, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ProcessDonePromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!currentIntent.Equals("Done"))
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }

            var choiceResult = (stepContext.Result as FoundChoice).Value;
            switch (choiceResult)
            {
                case "Return to editing query":
                    var continueEditingMessageText = "You can continue editing your query.";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, continueEditingMessageText, cancellationToken);

                case "Show result":
                    return await stepContext.EndDialogAsync(new { query = "Dummy query" });

                default:
                    var defaultExitMessage = "Something went wrong, clearing query and starting over. You have to specify the object type again!";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, defaultExitMessage, cancellationToken);
            }
        }
    }
}