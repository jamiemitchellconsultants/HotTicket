using Orleans;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketMessages;

namespace PublicTicketInterfaces
{
    public interface IPublicHold:IGrainWithGuidKey
    {
        Task<GrainResponse< HoldResponse>> GetHoldData();
        Task<GrainResponse<HoldResponse>> HoldSeat(Guid seat);
        Task<GrainResponse<HoldResponse>> HoldSeats(List<Guid> seats);
        Task<GrainResponse<TicketsMessage>> Sell();
        Task<GrainResponse<HoldResponse>> Release();
    }
}