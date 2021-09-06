using System;
using System.Collections.Generic;

namespace TicketMessages
{
    /// <summary>
    /// Communicates the response from a request to hold out of the silo
    /// </summary>
    public class HeldSeatsResponse:HoldResponse
    {

        public List<Guid> SeatsNotHeld { get; set; }
    }
}