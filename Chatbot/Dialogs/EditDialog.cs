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
    public class EditDialog : ParsingDialogBase
    {
        public EditDialog(
            ConversationState conversationState,
            ILogger<EditDialog> logger
            ) : base(nameof(EditDialog), conversationState, logger)
        {
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
            // ChoicePrompt helyett használhattok mást is (pl. sima Prompt-ot)
            var choices = new List<Choice>();
            foreach (var statement in conversationData.Statements)
            {
                choices.Add(new Choice(statement.Text));
            }

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
            conversationData.StatementToEdit = (stepContext.Result as FoundChoice).Value;

            var messageText = "What is the new statement?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> EditStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var newStatement = stepContext.Result.ToString();

            // TODO: itt lehet szerkeszteni

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}