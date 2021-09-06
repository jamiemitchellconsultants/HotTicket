
namespace HotTicket.Shared
{
    /// <summary>
    /// A sellable seat in an area for a performance
    /// </summary>
    public class SeatModel
    {   /// <summary>
        /// Area the seat is located
        /// </summary>
        /// <example>Red</example>
        public string Area { get; set; } = "";
        /// <summary>
        /// Name of the performance the seat is for
        /// </summary>
        /// <example>The big cup match</example>
        public string Performance { get; set; } = "";
        /// <summary>
        /// Unique Reference of a seat
        /// </summary>

        public string SeatId { get; set; } = "";
        /// <summary>
        /// The name/number of the physical seat in the area
        /// </summary>
        ///<example>Seat 5, Row A</example>
        public string PhysicalSeatId { get; set; } = "";
        /// <summary>
        /// Number of the physical seat
        /// </summary>
        public string PhysicalSeatNumber { get; set; } = "";
    }
}
