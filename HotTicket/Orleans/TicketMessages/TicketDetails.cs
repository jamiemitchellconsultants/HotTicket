using System;

namespace TicketMessages
{
    /// <summary>
    /// Representation of a ticket out of the silo
    /// </summary>
    public class TicketDetails
    {
        public string SeatNumber { get; set; }
        public string EntryCode { get; set; }
        public Guid TicketId { get; set; }
    };
}