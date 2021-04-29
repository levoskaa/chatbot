using Chatbot.CognitiveModels;
using Chatbot.Extensions;
using Chatbot.Interfaces;
using Chatbot.Models;
using Microsoft.Bot.Builder;
using SqlKata;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Chatbot.CognitiveModels.SimpleModel._Entities;

namespace Chatbot.Utility
{
    public class SimpleQueryHandler : QueryHandlerBase, ISimpleQueryHandler
    {
        public SimpleQueryHandler(ConversationState conversationState, QueryFactory queryFactory)
            : base(conversationState, queryFactory)
        {
        }

        public async Task<string> AddObjectTypeAsync(SimpleModel luisResult, ITurnContext context)
        {
            var objectType = luisResult.Entities.subject[0];
            // TODO: handle table name synonyms
            // TODO: handle plural/singular forms
            // TODO: handle letter casing
            var conversationData = await conversationStateAccessors.GetAsync(context, () => new ConversationData());
            if (conversationData.Query == null)
            {
                conversationData.Query = new Query();
            }
            objectType = objectType.FirstCharToUpper() + "s";
            conversationData.Query.From(objectType);
            return objectType;
        }

        public async Task<string> AddStatementAsync(SimpleModel luisResult, ITurnContext context)
        {
            // var statement
            var convertsationData = await conversationStateAccessors.GetAsync(context, () => new ConversationData());
            var statement = ParseLuisResult(luisResult);

            if (statement.MultipleValues)
            {
                if (statement.DateValues)
                {
                    List<DateTime> vals = new List<DateTime>();
                    if (!DateTime.TryParse(statement.Value[0], out DateTime date1))
                        throw new Exception("Can't convert the given string to DateTime!");
                    if (!DateTime.TryParse(statement.Value[1], out DateTime date2))
                        throw new Exception("Can't convert the given string to DateTime!");
                    vals.Add(date1);
                    vals.Add(date2);

                    var values = vals.ToArray();
                    if (statement.Negated)
                    {
                        convertsationData.Query.WhereNotBetween(statement.Property, values.Min(), values.Max());
                    }
                    else
                    {
                        convertsationData.Query.WhereBetween(statement.Property, values.Min(), values.Max());
                    }
                }
                else
                {
                    var values = Array.ConvertAll(statement.Value, item => double.Parse(item));
                    if (statement.Negated)
                    {
                        convertsationData.Query.WhereNotBetween(statement.Property, values.Min(), values.Max());
                    }
                    else
                    {
                        convertsationData.Query.WhereBetween(statement.Property, values.Min(), values.Max());
                    }
                }
            }
            else
            {
                if (statement.Negated)
                {
                    if (statement.Bigger)
                    {
                        convertsationData.Query.WhereNot(statement.Property, ">", statement.Value);
                    }
                    else if (statement.Smaller)
                    {
                        convertsationData.Query.WhereNot(statement.Property, "<", statement.Value);
                    }
                    else
                    {
                        // TODO: works with numbers and dates, but with strings we need to use LIKE
                        // this depends on the type of statement.Property in the database
                        convertsationData.Query.WhereNot(statement.Property, "=", statement.Value);
                    }
                }
                else
                {
                    if (statement.Bigger)
                    {
                        convertsationData.Query.Where(statement.Property, ">", statement.Value);
                    }
                    else if (statement.Smaller)
                    {
                        convertsationData.Query.Where(statement.Property, "<", statement.Value);
                    }
                    else
                    {
                        // TODO: works with numbers and dates, but with strings we need to use LIKE
                        // this depends on the type of statement.Property in the database
                        convertsationData.Query.Where(statement.Property, "=", statement.Value);
                    }
                }
            }
            //convertsationData.Query.Where
            return "";
        }

        private Statement ParseLuisResult(SimpleModel luisResult)
        {
            SimpleModel._Entities Entities = luisResult.Entities;
            ValueClass firstValue = Entities.value?.FirstOrDefault();
            string subject = Entities.subject?.FirstOrDefault();
            string property = Entities.obj?.FirstOrDefault();
            bool negated = Entities.negate?.FirstOrDefault() != null;
            bool bigger = Entities.bigger?.FirstOrDefault() != null;
            bool smaller = Entities.smaller?.FirstOrDefault() != null;
            bool multipleValues = false;
            bool dateValues = false;
            string[] values = { Entities.datetime?.FirstOrDefault()?.Expressions?.FirstOrDefault() ?? firstValue?.geography?.FirstOrDefault()?.ToString() ?? firstValue?.number?.FirstOrDefault().ToString() ?? firstValue?.personName?.FirstOrDefault() ?? firstValue?.text?.FirstOrDefault() };

            if (firstValue != null && (firstValue?.date != null || Entities?.datetime != null && firstValue.isEmpty()))
                dateValues = true;

            if (Entities?.around?.FirstOrDefault() != null)
            {
                List<string> vals = new List<string>();

                if (dateValues)
                {
                    var dateString = Entities.datetime?.FirstOrDefault()?.Expressions?.FirstOrDefault() ??
                                      Entities?.datetime?.FirstOrDefault()?.Expressions?.FirstOrDefault();

                    if (!DateTime.TryParse(dateString, out DateTime date))
                        throw new Exception("Can't convert the given string to DateTime!");

                    vals.Add(date.AddYears(-1).ToShortDateString());
                    vals.Add(date.AddYears(1).ToShortDateString());
                }
                else
                {
                    vals.Add((firstValue?.number?.First() * 0.8).ToString());
                    vals.Add((firstValue?.number?.First() * 1.2).ToString());
                }

                values = vals.ToArray();
                multipleValues = true;
            }
            else if (Entities?.between?.FirstOrDefault() != null || dateValues)
            {
                List<string> vals = new List<string>();

                if (dateValues)
                {
                    var dateStrings = Entities.datetime?.FirstOrDefault()?.Expressions ?? Entities?.datetime?.FirstOrDefault()?.Expressions.FirstOrDefault().Split(",");

                    if (dateStrings.Count > 1)
                    {
                        vals.Add(dateStrings.First().Remove(0, 1));
                        vals.Add(dateStrings[1]);
                    }
                    else
                        vals.Add(dateStrings.First());
                }
                else
                {
                    vals.Add((firstValue?.number?.First()).ToString());
                    vals.Add((firstValue?.number?.Last()).ToString());
                }

                if (vals.Count > 1)
                    multipleValues = true;

                values = vals.ToArray();
            }

            return new Statement
            {
                Subject = subject,
                Property = property,
                Negated = negated,
                Bigger = bigger,
                Smaller = smaller,
                Value = values,
                MultipleValues = multipleValues,
                DateValues = dateValues
            };
        }
    }
}