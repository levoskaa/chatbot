using Chatbot.Interfaces;
using SqlKata;
using System.Collections.Generic;

namespace Chatbot.Utility
{
    public abstract class QueryHandlerBase : IQueryHandler
    {
        protected List<string> constraints = new List<string>();
        public IReadOnlyList<string> Constraints => constraints.AsReadOnly();

        // TODO: Singleton osztály, query-t/constraint-eket nem szabad ebben tárolni
        protected Query query = new Query();
    }
}