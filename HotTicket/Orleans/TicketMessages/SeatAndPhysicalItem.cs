using System;

namespace TicketMessages
{
    public class SeatAndPhysicalItem
    {
        public Guid SeatId { get; set; }
        public Guid PhysicalSeatId { get; set; }
        public string PhysicalSeatNumber { get; set; }
    }
}
