using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketMessages;
namespace PublicTicketInterfaces
{
    public interface IPublicArea:IGrainWithStringKey
    {
        Task<GrainResponse<AreaDetails>> GetAreaData();
        Task<GrainResponse> InitialisePhysicalSeats(int seatCount);
        Task<GrainResponse> CreatePerformance(string performanceName);
        Task<GrainResponse< AvailableSeatsMessage>> GetAvailableSeats(string performanceName);
        Task<GrainResponse< PerformanceList>> GetPerformanceList();
        Task<GrainResponse<HeldSeatsResponse>> HoldSeats(string performanceName, List<Guid> seats);
        Task<GrainResponse< TicketsMessage>> SellSeat(Guid holdId);
    }
}