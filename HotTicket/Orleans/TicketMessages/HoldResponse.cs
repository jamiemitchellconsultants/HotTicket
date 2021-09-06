using System;
using System.Collections.Generic;

namespace TicketMessages
{
    public class HoldResponse
    {
        public Guid HoldId { get; set; }
        public List<Guid> SeatsHeld { get; set; }
    }
}
