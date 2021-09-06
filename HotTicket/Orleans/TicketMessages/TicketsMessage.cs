using System.Collections.Generic;

namespace TicketMessages
{
    /// <summary>
    /// Communicate a list of tickets "out" of the silo
    /// </summary>
    public class TicketsMessage
    {
        public string AreaName { get; set; }
        public string PerformanceName { get; set; }
        public List<TicketDetails> Tickets { get; set; }
    }
}