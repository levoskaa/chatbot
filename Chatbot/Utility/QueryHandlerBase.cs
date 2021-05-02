using Chatbot.Interfaces;
using Chatbot.Models;
using Microsoft.Bot.Builder;
using SqlKata;
using SqlKata.Execution;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chatbot.Utility
{
    public abstract class QueryHandlerBase : IQueryHandler
    {
        protected readonly IStatePropertyAccessor<ConversationData> conversationStateAccessors;
        protected readonly QueryFactory queryFactory;

        public QueryHandlerBase(ConversationState conversationState, QueryFactory queryFactory)
        {
            conversationStateAccessors = conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            this.queryFactory = queryFactory;
        }

        public async Task<List<string>> GetStatementsAsync(ITurnContext context)
        {
            // var conversationData = await conversationStateAccessors.GetAsync(context, () => new ConversationData());
            //return conversationData.Statements;
            return null;
        }

        public async Task<string> GetColumnType(ITurnContext context, string type)
        {

            var conversationData = await conversationStateAccessors.GetAsync(context, () => new ConversationData());
            var query = new Query();
            query.Select("DATA_TYPE")
                .From("INFORMATION_SCHEMA.COLUMNS")
                .Where("TABLE_NAME", "=", conversationData.CurrentTableName)
                .Where("COLUMN_NAME", "=", type);
            var xQuery = queryFactory.FromQuery(query);
            return xQuery.Get().FirstOrDefault();

        }
    }
}