using System.Collections.Generic;

namespace Chatbot.Interfaces
{
    public interface IQueryHandler
    {
        IReadOnlyList<string> Constraints { get; }
    }
}