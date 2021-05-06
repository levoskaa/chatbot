// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.11.1

using Chatbot.Extensions;
using Chatbot.Interfaces;
using Chatbot.Models;
using Chatbot.Utility;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using SqlKata;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chatbot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger logger;
        private readonly QueryFactory queryFactory;
        private readonly IComplexQueryHandler complexQueryHandler;
        private readonly ISimpleQueryHandler simpleQueryHandler;
        private readonly IStatePropertyAccessor<ConversationData> conversationStateAccessors;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(
            ComplexParsingDialog complexDialog,
            SimpleParsingDialog simpleDialog,
            ConversationState conversationState,
            QueryFactory queryFactory,
            IComplexQueryHandler complexQueryHandler,
            ISimpleQueryHandler simpleQueryHandler,
            ILogger<MainDialog> logger
            ) : base(nameof(MainDialog))
        {
            this.queryFactory = queryFactory;
            this.complexQueryHandler = complexQueryHandler;
            this.simpleQueryHandler = simpleQueryHandler;
            this.logger = logger;
            conversationStateAccessors = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));

            var waterfallSteps = new WaterfallStep[]
            {
                QueryParsingStepAsync,
                ShowResultStepAsync,
                FinalStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(complexDialog);
            AddDialog(simpleDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> QueryParsingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = stepContext.Options as MainDialogOptions ?? new MainDialogOptions();
            var conversationData = await conversationStateAccessors.GetAsync(stepContext.Context, () => new ConversationData());
            //conversationData.CurrentDb = db;
            if (conversationData.ModelBeingUsed == CognitiveModel.Simple)
            {
                return await stepContext.BeginDialogAsync(nameof(SimpleParsingDialog), options.Message, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(ComplexParsingDialog), options.Message, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ShowResultStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = await conversationStateAccessors.GetAsync(stepContext.Context, () => new ConversationData());

            // If the child dialog ("ComplexParsingDialog" or "SimpleParsingDialog") was cancelled or something went wrong,
            // the Result here will be null.
            // TODO: delete true after ComplexParsingDialog returns a value
            if (true || stepContext.Result != null /*is BookingDetails result*/)
            {
                // Now we have all the constraints, we can execute the query.
                var result = await ExecuteQueryAsync(conversationData, stepContext.Context);
                await DisplayQueryResults(result, stepContext.Context, cancellationToken);

                var choices = new List<Choice>
                {
                    new Choice("Yes, create new query"),
                    new Choice("No, modify query")
                };
                if (conversationData.ModelBeingUsed == CognitiveModel.Simple)
                {
                    choices.Add(new Choice("Switch to complex chatbot"));
                }
                else
                {
                    choices.Add(new Choice("Switch to stricter chatbot"));
                }

                var card = DialogHelper.CreateChoiceCard(choices, "Are you satisfied with the result?");
                var cardActivity = (Activity)card.CreateActivity();

                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = cardActivity,
                    RetryPrompt = cardActivity,
                    Choices = choices,
                    Style = ListStyle.None
                }, cancellationToken);
            }
            else
            {
                var messageText = "Something went wrong, clearing query and starting over.";
                return await stepContext.ReplaceDialogAsync(InitialDialogId, messageText, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var conversationData = await conversationStateAccessors.GetAsync(stepContext.Context, () => new ConversationData());
            var options = new MainDialogOptions();
            var choiceResult = (stepContext.Result as FoundChoice).Value;
            switch (choiceResult)
            {
                case "Yes, create new query":
                    options.Message = "What else are you looking for?";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, options, cancellationToken);

                case "No, modify query":
                    options.Message = "You can continue editing your query.";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, options, cancellationToken);

                case "Switch to stricter chatbot":
                    conversationData.ModelBeingUsed = CognitiveModel.Simple;
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);

                case "Switch to complex chatbot":
                    conversationData.ModelBeingUsed = CognitiveModel.Complex;
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);

                default:
                    options.Message = "Something went wrong, clearing query and starting over. You have to specify the object type again!";
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, options, cancellationToken);
            }
        }

        private async Task<IEnumerable<dynamic>> ExecuteQueryAsync(ConversationData conversationData, ITurnContext context)
        {
            var query = new Query();
            foreach (var statement in conversationData.Statements)
            {
                if (conversationData.ModelBeingUsed == CognitiveModel.Complex)
                {
                    await complexQueryHandler.AddStatementAsync(statement, query, context);
                }
                else
                {
                    await simpleQueryHandler.AddStatementAsync(statement, query, context);
                }
            }

            var xQuery = queryFactory.FromQuery(conversationData.Query);
            return xQuery.Get();
        }

        private async Task DisplayQueryResults(IEnumerable<dynamic> result, ITurnContext context, CancellationToken cancellationToken)
        {
            string messageText;
            int i = 1;
            var sb = new StringBuilder();

            messageText = "Here are the results of your query...";
            await SendTextMessage(messageText, context, cancellationToken);

            foreach (var row in result)
            {
                sb.Append($"{i}. ");

                var properties = (IDictionary<string, object>)row;

                foreach (var property in properties)
                {
                    sb.Append($"{property.Key}: {property.Value}; ");
                }

                ++i;
                sb.Append(Environment.NewLine);
            }

            await SendTextMessage(sb.ToString(), context, cancellationToken);
        }

        private async Task SendTextMessage(string messageText, ITurnContext context, CancellationToken cancellationToken)
        {
            var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            await context.SendActivityAsync(message, cancellationToken);
        }
    }
}