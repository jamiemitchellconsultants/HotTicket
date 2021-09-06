using System;
using System.Collections.Generic;

namespace HotTicket.Shared
{
    /// <summary>
    /// Seats on hold during booking
    /// </summary>
    public class HoldModel
    {
        /// <summary>
        /// Unique id for a hold
        /// </summary>
        public Guid HoldId { get; set; }
        /// <summary>
        /// Seats on hold
        /// </summary>
        public List<Guid> SeatsHeld { get; set; }
    }
}