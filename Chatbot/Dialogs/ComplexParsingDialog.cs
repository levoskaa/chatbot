using AdaptiveCards;
using Chatbot.CognitiveModels;
using Chatbot.Recognizers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chatbot.Dialogs
{
    public class ComplexParsingDialog : CancelAndHelpDialog
    {
        private readonly ComplexStatementRecognizer complexRecognizer;
        private readonly SimpleStatementRecognizer simpleRecognizer;
        protected readonly ILogger logger;

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
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = stepContext.Options?.ToString() ?? "What do you want to do?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var complexResult = await complexRecognizer.RecognizeAsync<ComplexModel>(stepContext.Context, cancellationToken);
            var topIntent = complexResult.TopIntent().intent;

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
                      new Choice("Modify query")
                    };

                    var actions = choices.Select(choice => new AdaptiveSubmitAction
                    {
                        Title = choice.Value,
                        Data = choice.Value
                    }).ToList<AdaptiveAction>();

                    var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                    {
                        Body = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock
                            {
                                Text = "What do you want to do?",
                                Wrap = true,
                            },
                        },
                        Actions = actions
                    };

                    Activity cardActivity = (Activity)MessageFactory.Attachment(new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = JObject.FromObject(card),
                    });

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
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, "Exit intent recognized", cancellationToken);

                case ComplexModel.Intent.Help:
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, "Help intent recognized", cancellationToken);

                default:
                    var noIntentMessage = "Sorry, I could not understand that.";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, noIntentMessage, cancellationToken);
            }
        }
    }
}