using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggeredEmailer.Helpers
{
    public class AppSettings
    {
        public List<FileLogger> FileLogger { get; set; } = new List<FileLogger>();
    }

    public class FileLogger
    {
        public LogProperties Billing { get; set; } = new LogProperties();
    }

    public class LogProperties
    {
        public string? Path { get; set; }
    }

}
