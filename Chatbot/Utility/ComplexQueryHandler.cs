﻿using Chatbot.CognitiveModels;
using Chatbot.Interfaces;
using Chatbot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Chatbot.Utility
{
    public class ComplexQueryHandler : QueryHandlerBase, IComplexQueryHandler
    {
        public ComplexQueryHandler(ConversationState conversationState)
            : base(conversationState)
        {
        }

        public Task<string> AddObjectTypeAsync(ComplexModel luisResult, ITurnContext context)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Statement> AddStatementAsync(ComplexModel luisResult, ITurnContext context)
        {
            var conversationData = await conversationStateAccessors.GetAsync(context,()=>new ConversationData());
            string addedStatementMessageText = "";
            string text = luisResult.Text;
            _Entities Entities = luisResult.Entities;
            _Entities.ClauseClass firstClause = Entities?.clause?.FirstOrDefault();
            _Entities.ValueClass firstValue = firstClause?.value?.FirstOrDefault();
            string referral = firstClause?.referral?.FirstOrDefault();
            string property = firstClause?.property?.FirstOrDefault();
            string[] values = { firstValue?.date?.FirstOrDefault()?.Expressions?.FirstOrDefault()?.ToString() ?? firstValue?.geography?.FirstOrDefault()?.ToString() ?? firstValue?.number?.FirstOrDefault().ToString() ?? firstValue?.personName?.FirstOrDefault() ?? firstValue?.text?.FirstOrDefault() };
            string objectType = firstClause?.subject?.FirstOrDefault();
            bool negated = firstClause?.negated?.FirstOrDefault() != null;
            bool bigger = firstClause?.bigger?.FirstOrDefault() != null;
            bool smaller = firstClause?.smaller?.FirstOrDefault() != null;
            bool multipleValues = false;
            bool dateValues = false;


            if (firstValue != null && (firstValue?.date != null || firstValue?.daterange != null )|| Entities?.datetime != null && firstValue.isEmpty())
                dateValues = true;

            if (dateValues)
            {
                List<string> vals = new List<string>();
                if (firstValue?.date != null)
                {
                    var dateString = firstValue.date?.FirstOrDefault()?.Expressions?.FirstOrDefault();
                    if (!DateTime.TryParse(dateString, out DateTime date))
                        throw new Exception("Can't convert the given string to DateTime!");
                    vals.Add(date.ToShortDateString());

                    values = vals.ToArray();
                }
                else if (firstValue?.daterange != null)
                {
                    var dateRangeString = firstValue.daterange?.FirstOrDefault()?.Expressions?.FirstOrDefault();
                    var resolution = TimexResolver.Resolve(new[] { dateRangeString });
                    string dateStart = resolution.Values[0].Start;
                    string dateEnd = resolution.Values[0].End;
                    if (!DateTime.TryParse(dateStart, out DateTime date1))
                        throw new Exception("Can't convert the given string to DateTime!");
                    if (!DateTime.TryParse(dateEnd, out DateTime date2))
                        throw new Exception("Can't convert the given string to DateTime!");
                    vals.Add(date1.ToShortDateString());
                    vals.Add(date2.ToShortDateString());

                    values = vals.ToArray();
                }

            }

            if (firstClause?.around?.FirstOrDefault() != null && firstValue?.daterange == null)
            {
                List<string> vals = new List<string>();
                vals.Add((firstValue?.number?.First() * 0.8).ToString());
                vals.Add((firstValue?.number?.First() * 1.2).ToString());

                values = vals.ToArray();
                multipleValues = true;
            }
            else if (firstClause?.between?.FirstOrDefault() != null)
            {
                List<string> vals = new List<string>();
                vals.Add((firstValue?.number?.First()).ToString());
                vals.Add((firstValue?.number?.Last()).ToString());

                if (vals.Count > 1)
                    multipleValues = true;

                values = vals.ToArray();
            }

            if (!string.IsNullOrEmpty(property) && string.IsNullOrEmpty(values.FirstOrDefault()))
            {
                string nullValueErrorMessage = "I didn't understand the value, please give me the whole sentence again, but try putting the value between quotation marks!";
                return null;
            }

            if (string.IsNullOrEmpty(property))
            {
                string propertyError = "I didn't understand the property, please enter another constraint!";
                return null;
            }

            string optionalNegate = negated ? "not " : "";
            string optionalSmallerBigger = smaller ? "smaller than " : (bigger ? "bigger than " : "");
            string optionalBetween = multipleValues ? "between " : "";
            string optionalSecondValue = multipleValues ? " and " + values.LastOrDefault() : "";

            if (string.IsNullOrEmpty(conversationData.SpecifiedObjectType) && objectType == null)
            {
                if (string.IsNullOrEmpty(objectType))
                {
                    var promptMessage = "The type of the object was not specified, please enter another constraint containing it!";
                    return null;
                }
                else if (objectType.Equals("he") || objectType.Equals("she") || objectType.Equals("his") || objectType.Equals("her") || objectType.Equals("its") || objectType.Equals("it"))
                {
                    var unknownTypeMessageText = $"I don't know what you are referring to by \"{objectType}\", please enter another contstraint containing the type of the object you are looking for!";
                    return  null;
                }
                else
                {
                    //query.ObjectType = objectType;
                    addedStatementMessageText = $"A {objectType}, got it!" + Environment.NewLine +
                                                $"Recognised constraint: {property} - {optionalNegate}{optionalSmallerBigger}{optionalBetween}{values.FirstOrDefault()}{optionalSecondValue}." + Environment.NewLine +
                                                $"You can give me more sentences, with additional constraints.";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(objectType) && !objectType.Equals(conversationData.SpecifiedObjectType))
                {
                    //var promptMessage = $"You already specified the object type: \"{query.ObjectType}\", please enter another constraint and refer to the object by the given type!";
                    var promptMessage = $"You alredy specified the object type";
                    return null;
                }
                else
                {
                    addedStatementMessageText = $"Recognised constraint: {property} - {optionalNegate}{optionalSmallerBigger}{optionalBetween}{values.FirstOrDefault()}{optionalSecondValue}." + Environment.NewLine +
                                                $"You can continue adding constraints, or try executing the query!";
                }
            }

            Statement stmnt = new Statement
            {
                Property = property,
                Value = values,
                Text = text,
                Bigger = bigger,
                Smaller = smaller,
                Negated = negated,
                MultipleValues = multipleValues,
                DateValues = dateValues,
                Subject = objectType
            };

            return stmnt;
        }
    }
}