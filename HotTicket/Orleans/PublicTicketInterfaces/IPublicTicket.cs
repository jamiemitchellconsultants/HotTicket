using System.Threading.Tasks;
using Orleans;
using TicketMessages;

namespace PublicTicketInterfaces
{
    /// <summary>
    /// Representation of a ticket where a ticket is the token that permits entry to a performance
    /// </summary>
    public interface IPublicTicket:IGrainWithGuidKey
    {
        /// <summary>
        /// The code that the outside world uses to track the ticket
        /// </summary>
        /// <returns></returns>
        Task<GrainResponse< SingleTicket>> GetDetails();



    }
}