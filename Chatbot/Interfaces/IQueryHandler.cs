using System.Collections.Generic;

namespace Chatbot.Interfaces
{
    public interface IQueryHandler
    {
        List<string> Constraints { get; }
    }
}