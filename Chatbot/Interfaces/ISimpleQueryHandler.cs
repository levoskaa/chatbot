using Chatbot.CognitiveModels;

namespace Chatbot.Interfaces
{
    public interface ISimpleQueryHandler : IQueryHandler
    {
        void AddObjectType(SimpleModel luisResult);

        void AddConstraint(SimpleModel luisResult);
    }
}