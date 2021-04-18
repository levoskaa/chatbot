using Chatbot.CognitiveModels;

namespace Chatbot.Interfaces
{
    public interface IComplexQueryHandler : IQueryHandler
    {
        void AddConstraint(ComplexModel luisResult);
    }
}