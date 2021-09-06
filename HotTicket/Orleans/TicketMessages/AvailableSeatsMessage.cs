namespace TicketMessages
{
    /// <summary>
    /// Communicates the seats available out of the silo
    /// </summary>
    public class AvailableSeatsMessage : SeatsMessage
    {
        public bool PerformanceNotFound { get; set; }
    }
}