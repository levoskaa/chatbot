using Chatbot.CognitiveModels;
using Chatbot.Interfaces;

namespace Chatbot.Utility
{
    public class SimpleQueryHandler : QueryHandlerBase, ISimpleQueryHandler
    {
        public string AddObjectType(SimpleModel luisResult)
        {
            var objectType = luisResult.Entities.subject[0];
            // TODO: handle table name synonyms
            // TODO: handle plural/singular forms
            // TODO: handle letter casing
            query.From(objectType);
            return objectType;
        }

        public void AddConstraint(SimpleModel luisResult)
        {
            throw new System.NotImplementedException();
        }
    }
}