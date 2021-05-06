using Chatbot.CognitiveModels;
using Chatbot.Extensions;
using Chatbot.Interfaces;
using Chatbot.Models;
using Chatbot.Recognizers;
using Chatbot.Utility;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chatbot.Dialogs
{
    public class ComplexParsingDialog : ParsingDialogBase
    {
        private readonly ComplexStatementRecognizer complexRecognizer;
        private readonly IComplexQueryHandler queryHandler;

        public ComplexParsingDialog(
            ComplexStatementRecognizer complexRecognizer,
            IComplexQueryHandler queryHandler,
            EditDialog editDialog,
            ConversationState conversationState,
            ILogger<ComplexParsingDialog> logger
            ) : base(nameof(ComplexParsingDialog), conversationState, logger)
        {
            this.complexRecognizer = complexRecognizer;
            this.queryHandler = queryHandler;

            var waterfallSteps = new WaterfallStep[]
            {
                PromptStepAsync,
                ActStepAsync,
                ProcessDonePromptAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(editDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string messageText;
            var conversationData = await conversationStateAccessors.GetAsync(stepContext.Context, () => new ConversationData());
            var complexResult = await complexRecognizer.RecognizeAsync<ComplexModel>(stepContext.Context, cancellationToken);
            var topIntent = complexResult.TopIntent().intent;
            conversationData.CurrentIntent = topIntent.ToString();

            switch (topIntent)
            {
                case ComplexModel.Intent.Statement:
                    string s = await queryHandler.AddStatementAsync(complexResult, stepContext.Context);
                    messageText = s;
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);

                case ComplexModel.Intent.ObjectType:
                    string ss = await queryHandler.AddObjectTypeAsync(complexResult, stepContext.Context);
                    conversationData.SpecifiedObjectType = ss;
                    if (conversationData.SpecifiedObjectType == null)
                        messageText = "Coulnd't understand the type you specified.";
                    else
                        messageText = "Got it, you are looking for a " + conversationData.SpecifiedObjectType + ".";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);

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

                case ComplexModel.Intent.List:
                    await DisplayQuery(conversationData, stepContext.Context, cancellationToken);
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, "You can continue to add constraints, edit them or execute the query.", cancellationToken);

                case ComplexModel.Intent.Delete:
                    messageText = "Delete intent recognized";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);

                case ComplexModel.Intent.Edit:
                    return await stepContext.BeginDialogAsync(nameof(EditDialog), null, cancellationToken);

                case ComplexModel.Intent.Exit:
                    return await stepContext.EndDialogAsync(null, cancellationToken);

                case ComplexModel.Intent.Help:
                    messageText = "Help intent recognized";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);

                default:
                    messageText = "Sorry, I could not understand that.";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ProcessDonePromptAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = await conversationStateAccessors.GetAsync(stepContext.Context, () => new ConversationData());
            if (!conversationData.CurrentIntent.Equals("Done"))
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
                    return await stepContext.EndDialogAsync(new { query = "Dummy query" });

                default:
                    messageText = "Something went wrong, clearing query and starting over. You have to specify the object type again!";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);
            }
        }
    }
}