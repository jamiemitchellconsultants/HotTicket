using System;

namespace HotTicket.Shared
{
    /// <summary>
    /// Model of a sale for either a single seat or all seats in a hold
    /// </summary>
    public class SaleModel
    {
        /// <summary>
        /// The id of the hold containing the seats to sell
        /// </summary>
        public Guid? HoldId { get; set; }
        /// <summary>
        /// The id of an unsold seat to sell
        /// </summary>
        public Guid? SeatId { get; set; }
    }
}