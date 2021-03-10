using Chatbot.Recognizers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace Chatbot.Dialogs
{
    public class SimpleParsingDialog : CancelAndHelpDialog
    {
        private readonly SimpleStatementRecognizer simpleRecognizer;
        protected readonly ILogger logger;

        public SimpleParsingDialog(SimpleStatementRecognizer simpleRecognizer, ILogger<SimpleParsingDialog> logger)
            : base(nameof(SimpleParsingDialog))
        {
            this.simpleRecognizer = simpleRecognizer;
            this.logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
    }
}