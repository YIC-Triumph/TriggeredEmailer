using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggeredEmailer.Constants;
using TriggeredEmailer.Helpers;
using TriggeredEmailer.Interfaces;

namespace TriggeredEmailer.Services
{
    public class FileLoggerService<T> : IFileLogger<T> where T : class
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<T> _logger;
        public FileLoggerService(
            IOptions<AppSettings> options,
            ILogger<T> logger)
        {
            _appSettings = options.Value;
            _logger = logger;
        }

        //Fields
        private string? _message;
        private LogType _logType;
        private LogLevel _logLevel;
        private object[]? _objects;

        /// <summary>
        /// Write log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logType"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        public async Task WriteLog(string message, LogType logType, LogLevel level, params object[] objects)
        {
            _logType = logType;
            _logLevel = level;
            _objects = objects;
            _message = message;

            switch (logType)
            {
                case LogType.Console:
                    WriteToConsole();
                    break;
                case LogType.File:
                    await WriteToFile();
                    break;
            }
        }

        /// <summary>
        /// Write to console
        /// </summary>
        private async void WriteToConsole()
        {
            _logger.Log(_logLevel, _message, _objects);
        }

        private async Task WriteToFile()
        {
            string path = string.Empty;
            string filename = DateTime.Now.ToString("yyyyMMdd");

            switch (typeof(T).Name)
            {
                case nameof(BillingService):
                    path = _appSettings.FileLogger.Select(n => n.Billing).FirstOrDefault().Path ?? string.Empty;
                    filename = $"Billing_{filename}";
                    break;
            }

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string fullName = Path.Combine($"{path}\\{filename}.txt");

            using StreamWriter sw = File.AppendText(fullName);
            var sb = new StringBuilder();

            sb.AppendLine($"\r\nLog Entry : {_logLevel}");
            sb.AppendLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            sb.AppendLine($"  :{_message}");
            sb.AppendLine("-------------------------------");

            await sw.WriteLineAsync(sb.ToString());
        }
    }
}
