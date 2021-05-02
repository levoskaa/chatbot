using Chatbot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Chatbot.Dialogs
{
    public abstract class ParsingDialogBase : CancelAndHelpDialog
    {
        protected readonly IStatePropertyAccessor<ConversationData> conversationStateAccessors;
        protected readonly ILogger logger;

        public ParsingDialogBase(string id, ConversationState conversationState, ILogger<ParsingDialogBase> logger)
            : base(id)
        {
            this.logger = logger;
            conversationStateAccessors = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        protected virtual async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = stepContext.Options?.ToString() ?? "What are you looking for?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        protected async Task SendTextMessage(string messageText, ITurnContext context, CancellationToken cancellationToken)
        {
            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await context.SendActivityAsync(message, cancellationToken);
        }
    }
}