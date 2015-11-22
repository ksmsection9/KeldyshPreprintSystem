using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeldyshPreprintSystem.Models
{

    public class LogEntry
    {
        public string UserRole { get; set; }
        public string UserName { get; set; }
        public bool StatusChanged { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public int subLogId { get; set; }
    }

    public class PanelContextModel
    {
        public List<LogEntry> Log {get; set;}
        public int paperId { get; set; }
        public bool smsForbidden { get; set; }
    }
}