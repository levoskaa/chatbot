using Chatbot.CognitiveModels;

namespace Chatbot.Interfaces
{
    public interface ISimpleQueryHandler : IQueryHandler
    {
        void AddConstraint(SimpleModel luisResult);
    }
}