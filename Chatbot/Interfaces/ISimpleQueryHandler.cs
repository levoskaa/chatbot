using Chatbot.CognitiveModels;

namespace Chatbot.Interfaces
{
    public interface ISimpleQueryHandler : IQueryHandler
    {
        string AddObjectType(SimpleModel luisResult);

        void AddConstraint(SimpleModel luisResult);
    }
}