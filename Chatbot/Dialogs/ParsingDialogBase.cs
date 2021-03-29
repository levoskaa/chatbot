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
        protected readonly ILogger logger;
        protected string currentIntent;

        public ParsingDialogBase(string id, ILogger<ParsingDialogBase> logger)
            : base(id)
        {
            this.logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        protected virtual async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var messageText = stepContext.Options?.ToString() ?? "What are you looking for?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }
    }
}