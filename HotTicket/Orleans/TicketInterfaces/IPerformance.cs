using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using PublicTicketInterfaces;
using TicketMessages;

namespace TicketInterfaces
{
    /// <summary>
    /// Represents a "thing that happens at a venue"
    /// </summary>
    public interface IPerformance : IGrainWithGuidKey,IPublicPerformance
    {
        /// <summary>
        /// Like a constructor
        /// </summary>
        /// <param name="area">The area of the venue where the perfomance takes place</param>
        /// <param name="performanceName">Display name of the performance</param>
        /// <param name="seats">List of physical seats that will be sellable</param>
        /// <returns></returns>
        Task<GrainResponse> InitialisePerformance(IArea area, string performanceName, List<IPhysicalSeat> seats);
        /// <summary>
        /// Gets the list of seats that are available to be sold
        /// </summary>
        /// <returns></returns>
        Task<GrainResponse< List<ISeat>>> GetAvailableSeats();
        /// <summary>
        /// Record that a seat is no longer available
        /// </summary>
        /// <param name="seat">Seat grain</param>
        /// <returns></returns>
        Task<GrainResponse> MarkSeatNotAvailable(ISeat seat);
        Task<GrainResponse> MarkSeatAvailable(ISeat seat);

    }
}