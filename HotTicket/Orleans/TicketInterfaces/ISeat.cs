using System.Threading.Tasks;
using Orleans;
using PublicTicketInterfaces;
using TicketMessages;

namespace TicketInterfaces
{
    /// <summary>
    /// A sell-able seat for a performance
    /// </summary>
    public interface ISeat:IGrainWithGuidKey,IPublicSeat
    {
        Task<GrainResponse> InitializeSeat(IArea area, IPerformance performance, IPhysicalSeat physicalSeat);
        /// <summary>
        /// Add a seat to a hold
        /// </summary>
        /// <param name="hold">Hold the seat will be added to</param>
        /// <returns>True for success</returns>
        Task<GrainResponse> HoldSeat(IHold hold);
        /// <summary>
        /// Sell a seat without a hold
        /// </summary>
        /// <returns>The ticket created</returns>
        Task<GrainResponse< ITicket>> SellSeat();
        /// <summary>
        /// Sell a held seat
        /// </summary>
        /// <param name="hold">the hold containing the seat</param>
        /// <returns></returns>
        Task<GrainResponse< ITicket>> SellSeat(IHold hold );
        Task<GrainResponse<InternalSeatData>> GetInternalData(bool includeTicket=false);
        Task<GrainResponse<IPerformance>> GetPerformance();
        Task<GrainResponse> ReleaseHold(IHold hold);
    }
}