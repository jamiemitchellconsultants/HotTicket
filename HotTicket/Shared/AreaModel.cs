using System.Collections.Generic;

namespace HotTicket.Shared
{
    /// <summary>
    /// Representation of an area of physical seats
    /// </summary>
    public record AreaModel
    {
        /// <summary>
        /// Name of the area
        /// </summary>
        /// <example>Red</example>
        public string AreaName { get; set; } = "";
        /// <summary>
        /// Seats in the Area
        /// </summary>
        /// <example>100</example>
        public int NumberOfSeats { get; set; }
        /// <summary>
        /// Performances booked for the area
        /// </summary>
        /// <example>["The Big Cup Final","Super Music Festival"]</example>
        public List<string> Performances { get; set; } = new List<string>();
    }
}
