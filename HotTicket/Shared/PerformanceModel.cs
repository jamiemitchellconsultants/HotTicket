namespace HotTicket.Shared
{
    /// <summary>
    /// Description of a performance / match
    /// </summary>
    public record PerformanceModel
    {
        /// <summary>
        /// Name of the area where the performance occurs
        /// </summary>
        /// <example>Red</example>
        public string AreaName { get; set; } = "";
        /// <summary>
        /// Name of the performance
        /// </summary>
        /// <example>The Big Cup Final</example>
        public string PerformanceName { get; set; } = "";
    }
}
