using Microsoft.Extensions.Logging;
using TriggeredEmailer.Constants;

namespace TriggeredEmailer.Interfaces
{
    public interface IFileLogger<T>
    {
        Task WriteLog(string message, LogType logType, LogLevel level, params object[] objects);
    }
}