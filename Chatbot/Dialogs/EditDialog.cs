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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Chatbot.Dialogs
{
    public class EditDialog : ParsingDialogBase
    {
        private readonly ComplexStatementRecognizer complexRecognizer;
        private readonly IComplexQueryHandler queryHandler;

        public EditDialog(
            ComplexStatementRecognizer complexRecognizer,
            IComplexQueryHandler queryHandler,
            ConversationState conversationState,
            ILogger<EditDialog> logger
            ) : base(nameof(EditDialog), conversationState, logger)
        {
            this.complexRecognizer = complexRecognizer;
            this.queryHandler = queryHandler;

            var waterfallSteps = new WaterfallStep[]
           {
                SelectStepAsync,
                NewStatementStepAsync,
                EditStepAsync,
           };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = await conversationStateAccessors.GetAsync(stepContext.Context, () => new ConversationData());
            if (conversationData.Statements.Count == 0)
            {
                await SendTextMessage("No statements were given yet.", stepContext.Context, cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            var choices = new List<Choice>();
            foreach (var statement in conversationData.Statements)
            {
                choices.Add(new Choice(statement.Text));
            }
            choices.Add(new Choice("Finish editing"));

            var card = DialogHelper.CreateChoiceCard(choices, "Which constraint would you like to edit?");
            var cardActivity = (Activity)card.CreateActivity();

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = cardActivity,
                RetryPrompt = cardActivity,
                Choices = choices,
                Style = ListStyle.None
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> NewStatementStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = await conversationStateAccessors.GetAsync(stepContext.Context, () => new ConversationData());
            if ((stepContext.Result as FoundChoice).Value == "Finish editing")
            {
                conversationData.Edited = true;
                return await stepContext.EndDialogAsync("Finished editing, now you can continue to add constraints, edit them or execute the query", cancellationToken);
            }
            conversationData.StatementToEdit = (stepContext.Result as FoundChoice).Value;

            var messageText = "What is the new statement? (Or just say delete, if you want it gone)";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> EditStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string messageText;
            var newStatement = stepContext.Result.ToString();
            var conversationData = await conversationStateAccessors.GetAsync(stepContext.Context, () => new ConversationData());
            var complexResult = await complexRecognizer.RecognizeAsync<ComplexModel>(stepContext.Context, cancellationToken);
            var topIntent = complexResult.TopIntent().intent;
            Statement old = null;

            // TODO: itt lehet szerkeszteni

            switch (topIntent)
            {
                case ComplexModel.Intent.Statement:
                    messageText = await queryHandler.AddStatementAsync(complexResult, stepContext.Context);
                    foreach (var statement in conversationData.Statements)
                    {
                        if (statement.Text == conversationData.StatementToEdit)
                            old = statement;
                    }
                    conversationData.Statements.Remove(old);
                    conversationData.Edited = true;
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);

                case ComplexModel.Intent.Delete:
                    foreach (var statement in conversationData.Statements)
                    {
                        if (statement.Text == conversationData.StatementToEdit)
                            old = statement;
                    }
                    conversationData.Statements.Remove(old);
                    conversationData.Edited = true;
                    messageText = "Deleted constraint: " + conversationData.StatementToEdit;
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);

                case ComplexModel.Intent.Help:
                    messageText = "Help intent recognized";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);

                default:
                    messageText = "Sorry, I could not understand that.";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);
            }
        }
    }
}