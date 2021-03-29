using Chatbot.CognitiveModels;
using Chatbot.Extensions;
using Chatbot.Recognizers;
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
    public class SimpleParsingDialog : ParsingDialogBase
    {
        private readonly SimpleStatementRecognizer simpleRecognizer;
        private bool objectTypeKnown = false;

        public SimpleParsingDialog(SimpleStatementRecognizer simpleRecognizer, ILogger<SimpleParsingDialog> logger)
            : base(nameof(SimpleParsingDialog), logger)
        {
            this.simpleRecognizer = simpleRecognizer;

            var waterfallSteps = new WaterfallStep[]
            {
                PromptStepAsync,
                ObjectTypeStepAsync,
                ActStepAsync,
                ProcessDonePromptAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ObjectTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (objectTypeKnown)
            {
                return await stepContext.NextAsync(stepContext.Context, cancellationToken);
            }

            var simpleResult = await simpleRecognizer.RecognizeAsync<SimpleModel>(stepContext.Context, cancellationToken);
            var topIntent = simpleResult.TopIntent().intent;

            switch (topIntent)
            {
                case SimpleModel.Intent.searchsubject:
                    objectTypeKnown = true;
                    var promptMessage = MessageFactory.Text("Now tell me the the details using sentences in \"something is something\" form (e.g. the author is William Shakespeare)." +
                                                      " You can give me multiple sentences and when you are finished, just tell me (e.g. I am finished).", InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);

                default:
                    var messageText = "Sorry, I could not understand that.";
                    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(message, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string messageText;
            var simpleResult = await simpleRecognizer.RecognizeAsync<SimpleModel>(stepContext.Context, cancellationToken);
            var topIntent = simpleResult.TopIntent().intent;
            currentIntent = topIntent.ToString();

            switch (topIntent)
            {
                case SimpleModel.Intent.simplestatement:
                    messageText = "Simple: Statement intent recognized";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);

                case SimpleModel.Intent.finishedstatement:
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

                case SimpleModel.Intent.list:
                    messageText = "Simple: List intent recognized";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);

                default:
                    messageText = "Sorry, I could not understand that.";
                    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(message, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ProcessDonePromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!currentIntent.Equals("finishedstatement"))
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }

            string messageText;
            var choiceResult = (stepContext.Result as FoundChoice).Value;
            switch (choiceResult)
            {
                case "Return to editing query":
                    messageText = "You can continue editing your query.";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);

                case "Show result":
                    objectTypeKnown = false;
                    return await stepContext.EndDialogAsync(new { query = "Dummy query" });

                default:
                    messageText = "Something went wrong, clearing query and starting over. You have to specify the object type again!";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);
            }
        }
    }
}