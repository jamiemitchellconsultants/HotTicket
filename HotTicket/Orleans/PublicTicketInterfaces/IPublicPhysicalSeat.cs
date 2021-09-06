using System.Threading.Tasks;
using Orleans;
using TicketMessages;

namespace PublicTicketInterfaces
{
    /// <summary>
    /// A physical seat in an area. A physical seat has its own internal Id but also provides a human readable seat number
    /// </summary>
    public interface IPublicPhysicalSeat:IGrainWithGuidKey
    {
        /// <summary>
        /// Returns the seat number 
        /// </summary>
        /// <returns></returns>
        Task<GrainResponse< string>> GetSeatNumber();



    }
}