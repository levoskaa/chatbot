// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.11.1

using Chatbot.CognitiveModels;
using Chatbot.Recognizers;
using ChatBot.StatementModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Chatbot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ComplexStatementRecognizer complexRecognizer;
        private readonly SimpleStatementRecognizer simpleRecognizer;
        protected readonly ILogger Logger;
        private bool restarted = false;
        private Query query = new Query();

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ComplexStatementRecognizer complexRecognizer, SimpleStatementRecognizer simpleRecognizer, ComplexParsingDialog complexDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            this.complexRecognizer = complexRecognizer;
            this.simpleRecognizer = simpleRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(complexDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        protected (string msg, Statement stmnt) StatementHandler(ComplexModel complexLuisResult, SimpleModel simpleLuisResult, Query query)
        {
            string addedStatementMessageText = "";

            string objectType = complexLuisResult.FirstEntities.Subject;
            string property = complexLuisResult.FirstEntities.Property;
            string[] value = complexLuisResult.FirstEntities.Values;
            string text = complexLuisResult.Text;
            bool negated = complexLuisResult.FirstEntities.Negated || simpleLuisResult.FirstEntities.Negated;
            bool bigger = complexLuisResult.FirstEntities.Bigger || simpleLuisResult.FirstEntities.Bigger;
            bool smaller = complexLuisResult.FirstEntities.Smaller || simpleLuisResult.FirstEntities.Smaller;
            bool multipleValues = complexLuisResult.FirstEntities.MultipleValues || simpleLuisResult.FirstEntities.MultipleValues;
            bool dateValues = complexLuisResult.FirstEntities.DateValues || simpleLuisResult.FirstEntities.DateValues;

            if (string.IsNullOrEmpty(property) || string.IsNullOrEmpty(value.FirstOrDefault()))
            {
                property = simpleLuisResult.FirstEntities.Property;
                value = simpleLuisResult.FirstEntities.Values;
                negated = simpleLuisResult.FirstEntities.Negated;
                bigger = simpleLuisResult.FirstEntities.Bigger;
                smaller = simpleLuisResult.FirstEntities.Smaller;
                multipleValues = simpleLuisResult.FirstEntities.MultipleValues;
                dateValues = simpleLuisResult.FirstEntities.DateValues;
                text = simpleLuisResult.Text;
                objectType = null;
            }

            if (!string.IsNullOrEmpty(property) && string.IsNullOrEmpty(value.FirstOrDefault()))
            {
                string nullValueErrorMessage = "I didn't understand the value, please give me the whole sentence again, but try putting the value between quotation marks!";
                restarted = true;
                return (nullValueErrorMessage, null);
            }

            if (string.IsNullOrEmpty(property))
            {
                string propertyError = "I didn't understand the property, please enter another constraint!";
                restarted = true;
                return (propertyError, null);
            }

            string optionalNegate = negated ? "not " : "";
            string optionalSmallerBigger = smaller ? "smaller than " : (bigger ? "bigger than " : "");
            string optionalBetween = multipleValues ? "between " : "";
            string optionalSecondValue = multipleValues ? " and " + value.LastOrDefault() : "";

            if (string.IsNullOrEmpty(query.ObjectType))
            {
                if (string.IsNullOrEmpty(objectType))
                {
                    var promptMessage = "The type of the object was not specified, please enter another constraint containing it!";
                    restarted = true;
                    return (promptMessage, null);
                }
                else if (objectType.Equals("he") || objectType.Equals("she") || objectType.Equals("his") || objectType.Equals("her") || objectType.Equals("its") || objectType.Equals("it"))
                {
                    var unknownTypeMessageText = $"I don't know what you are referring to by \"{objectType}\", please enter another contstraint containing the type of the object you are looking for!";
                    restarted = true;
                    return (unknownTypeMessageText, null);
                }
                else
                {
                    query.ObjectType = objectType;
                    addedStatementMessageText = $"A {query.ObjectType}, got it!" + Environment.NewLine +
                                                $"Recognised constraint: {property} - {optionalNegate}{optionalSmallerBigger}{optionalBetween}{value.FirstOrDefault()}{optionalSecondValue}." + Environment.NewLine +
                                                $"You can give me more sentences, with additional constraints.";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(objectType) && !objectType.Equals(query.ObjectType) && !(objectType.Equals("he") || objectType.Equals("she") || objectType.Equals("his") || objectType.Equals("her") || objectType.Equals("its") || objectType.Equals("it")))
                {
                    var promptMessage = $"You already specified the object type: \"{query.ObjectType}\", please enter another constraint and refer to the object by the given type!";
                    restarted = true;
                    return (promptMessage, null);
                }
                else
                {
                    addedStatementMessageText = $"Recognised constraint: {property} - {optionalNegate}{optionalSmallerBigger}{optionalBetween}{value.FirstOrDefault()}{optionalSecondValue}." + Environment.NewLine +
                                                $"You can continue adding constraints, or try executing the query!";
                }
            }

            Statement stmnt = new Statement
            {
                Property = property,
                Value = value,
                Text = text,
                Bigger = bigger,
                Smaller = smaller,
                Negated = negated,
                MultipleValues = multipleValues,
                DateValues = dateValues
            };

            return (addedStatementMessageText, stmnt);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!complexRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?\nSay something like \"Book a flight from Paris to Berlin on March 22, 2020\"";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!complexRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
            }

            // Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
            var complexResult = await complexRecognizer.RecognizeAsync<ComplexModel>(stepContext.Context, cancellationToken);
            var simpleResult = await simpleRecognizer.RecognizeAsync<SimpleModel>(stepContext.Context, cancellationToken);
            switch (complexResult.TopIntent().intent)
            {
                case ComplexModel.Intent.Statement:
                    (string msg, Statement stmnt) = StatementHandler(complexResult, simpleResult, query);

                    if (stmnt != null)
                    {
                        query.AddStatement(stmnt);
                    }

                    return await stepContext.ReplaceDialogAsync(InitialDialogId, msg, cancellationToken);

                case ComplexModel.Intent.ObjectType:
                    string onlyObjectType = complexResult.FirstEntities.ObjectType;

                    if (string.IsNullOrEmpty(query.ObjectType))
                    {
                        if (string.IsNullOrEmpty(onlyObjectType))
                        {
                            var promptMessage = "The type of the object was not specified, please enter another constraint containing it!";
                            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
                        }

                        query.ObjectType = onlyObjectType;
                        var typeMessage = $"A {query.ObjectType}, got it!" + Environment.NewLine +
                                          $"Now give me sentences with the details!" + Environment.NewLine +
                                          $"You can give me more than one sentence and when you are finished, just say so.";
                        return await stepContext.ReplaceDialogAsync(InitialDialogId, typeMessage, cancellationToken);
                    }
                    else
                    {
                        var promptMessage = $"You already specified the object type: \"{query.ObjectType}\", please enter another constraint and refer to the object by the given type!";
                        return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
                    }


                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {complexResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        // Shows a warning if the requested From or To cities are recognized as entities but they are not in the Airport entity list.
        // In some cases LUIS will recognize the From and To composite entities as a valid cities but the From and To Airport values
        // will be empty if those entity values can't be mapped to a canonical item in the Airport.
        private static async Task ShowWarningForUnsupportedCities(ITurnContext context, FlightBooking luisResult, CancellationToken cancellationToken)
        {
            var unsupportedCities = new List<string>();

            var fromEntities = luisResult.FromEntities;
            if (!string.IsNullOrEmpty(fromEntities.From) && string.IsNullOrEmpty(fromEntities.Airport))
            {
                unsupportedCities.Add(fromEntities.From);
            }

            var toEntities = luisResult.ToEntities;
            if (!string.IsNullOrEmpty(toEntities.To) && string.IsNullOrEmpty(toEntities.Airport))
            {
                unsupportedCities.Add(toEntities.To);
            }

            if (unsupportedCities.Any())
            {
                var messageText = $"Sorry but the following airports are not supported: {string.Join(',', unsupportedCities)}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await context.SendActivityAsync(message, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            if (stepContext.Result is BookingDetails result)
            {
                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                var timeProperty = new TimexProperty(result.TravelDate);
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}