using System.Threading.Tasks;
using Orleans;
using TicketMessages;

namespace PublicTicketInterfaces
{
    /// <summary>
    /// Represents a "thing that happens at a venue"
    /// </summary>
    public interface IPublicPerformance : IGrainWithGuidKey
    {
        Task<GrainResponse< string>> GetAreaName();
        Task<GrainResponse< string>> GetPerformanceName();
        Task <GrainResponse<PerformanceData>> GetPerformanceData();
    }
}