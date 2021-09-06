using System;

namespace TicketMessages
{
    public class InternalSeatData
    {
        public Guid SeatId { get; set; }
        public Guid PhysicalSeatId { get; set; }
        public string PhysicalSeatName { get; set; }
    }
}
