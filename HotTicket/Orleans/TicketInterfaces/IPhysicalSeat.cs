using System.Threading.Tasks;
using Orleans;
using PublicTicketInterfaces;
using TicketMessages;

namespace TicketInterfaces
{
    /// <summary>
    /// A physical seat in an area. A physical seat has its own internal Id but also provides a human readable seat number
    /// </summary>
    public interface IPhysicalSeat:IGrainWithGuidKey,IPublicPhysicalSeat
    {

        /// <summary>
        /// Sets the seat number
        /// </summary>
        /// <param name="seatNumber"></param>
        /// <returns></returns>
        Task<GrainResponse> SetSeatNumber(string seatNumber);


    }
}