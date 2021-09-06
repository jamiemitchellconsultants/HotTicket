using System.Threading.Tasks;
using Orleans;
using TicketMessages;

namespace PublicTicketInterfaces
{
    /// <summary>
    /// A sell-able seat for a performance
    /// No public methods
    /// </summary>
    public interface IPublicSeat:IGrainWithGuidKey
    {
        Task<GrainResponse< SeatData>> GetSeat();
    }
}