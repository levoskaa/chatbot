using Chatbot.Recognizers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace Chatbot.Dialogs
{
    public class ComplexParsingDialog : CancelAndHelpDialog
    {
        private readonly ComplexStatementRecognizer complexRecognizer;
        protected readonly ILogger logger;

        public ComplexParsingDialog(ComplexStatementRecognizer complexRecognizer, ILogger<ComplexParsingDialog> logger)
            : base(nameof(ComplexParsingDialog))
        {
            this.complexRecognizer = complexRecognizer;
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