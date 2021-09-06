using System.Collections.Generic;

namespace TicketMessages
{
    /// <summary>
    /// Used to communicate "out" of the silo. 
    /// </summary>
    public class SeatsMessage
    {
        public string AreaName { get; set; }
        public string PerformanceName { get; set; }
        public List<SeatAndPhysicalItem> Seats { get; set; }
    }
}