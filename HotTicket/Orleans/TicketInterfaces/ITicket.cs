using System.Threading.Tasks;
using Orleans;
using PublicTicketInterfaces;
using TicketMessages;

namespace TicketInterfaces
{
    /// <summary>
    /// Representation of a ticket where a ticket is the token that permits entry to a performance
    /// </summary>
    public interface ITicket:IGrainWithGuidKey,IPublicTicket
    {
        /// <summary>
        /// The code that the outside world uses to track the ticket
        /// </summary>
        /// <returns></returns>
        //Task<GrainResponse< string>> GetEntryCode();

        Task<GrainResponse> Initialize(ISeat seat);
    }
}