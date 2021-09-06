using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using TicketInterfaces;
using TicketMessages;

namespace TicketGrains
{
    /// <summary>
    /// This comment is irrelevant as all dev is against the interface not the class
    /// </summary>
    public class TicketGrain:Grain,ITicket
    {
        private readonly IPersistentState<TicketState> _store;

        public TicketGrain([PersistentState("ticket", "SeatStore")] IPersistentState<TicketState> store)
        {
            _store = store;
        }
        

        public async Task<GrainResponse> Initialize(ISeat seat)
        {
            var performanceResponse =await  seat.GetPerformance();
            var performance = performanceResponse.Result;
            var perfNameResponse = await performance.GetPerformanceData();
            _store.State.PerformanceName = perfNameResponse.Result.PerformanceName;
            _store.State.AreaName = perfNameResponse.Result.AreaName;
            _store.State.Seat = seat;
            var seatDataResponse = await seat.GetInternalData();
            _store.State.PhysicalSeat=GrainFactory.GetGrain<IPhysicalSeat>( seatDataResponse.Result.PhysicalSeatId);
            _store.State.PhysicalSeatNumber = seatDataResponse.Result.PhysicalSeatName;
            await _store.WriteStateAsync();
            return GrainResponse.SuccessResponse();
        }

        public async Task<GrainResponse<SingleTicket>> GetDetails()
        {
            var ticket = new SingleTicket
            {
                AreaName = _store.State.AreaName,
                PerformanceName = _store.State.PerformanceName,
                SeatNumber = _store.State.PhysicalSeatNumber,
                EntryCode = this.GetPrimaryKey().ToString()
            };
            return await Task.FromResult( GrainResponse<SingleTicket>.SuccessResponse(ticket));
        }
    }
    [Serializable]
    public class TicketState
    {
        public string AreaName { get; set; }
        public string PerformanceName { get; set; }
        public IPhysicalSeat PhysicalSeat { get; set; }
        public string PhysicalSeatNumber { get; set; }
        public ISeat Seat { get; set; }
    }
}