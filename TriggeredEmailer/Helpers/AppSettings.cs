using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggeredEmailer.Helpers
{
    public class AppSettings
    {
        public string? EmailFrom { get; set; }
        public string? SendGridApiKey { get; set; }
        public string? Domain { get; set; }
    }
}
