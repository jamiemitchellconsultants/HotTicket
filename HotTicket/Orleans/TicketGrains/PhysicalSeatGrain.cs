using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using TicketInterfaces;
using TicketMessages;

namespace TicketGrains
{
    /// <summary>
    /// Represents a physical seat in an area
    /// </summary>
    public class PhysicalSeatGrain:Grain,IPhysicalSeat
    {
        private readonly IPersistentState<PhysicalSeatState> _store;

        /// <summary>
        /// Inject state manage
        /// </summary>
        /// <param name="store">State persistence</param>
        public PhysicalSeatGrain([PersistentState("physicalSeat", "SeatStore")] IPersistentState<PhysicalSeatState> store)
        {
            _store = store;
        }
        /// <summary>
        /// Gets the number of the physical seat
        /// </summary>
        /// <returns></returns>
        public async Task<GrainResponse< string>> GetSeatNumber()
        {
            if (string.IsNullOrEmpty( _store.State.SeatNumber ))
            {
                return await Task.FromResult(GrainResponse<string>.FailureResponse("Not Initialized"));
            }

            return await  Task.FromResult(GrainResponse<string>.SuccessResponse( _store.State.SeatNumber));
        }
        /// <summary>
        /// Sets the seat number. Maybe should be only setable once
        /// </summary>
        /// <param name="seatNumber"></param>
        /// <returns></returns>
        public async Task<GrainResponse> SetSeatNumber(string seatNumber)
        {
            _store.State.SeatNumber=seatNumber;
            await _store.WriteStateAsync();
            return GrainResponse.SuccessResponse();
        }
    }
    /// <summary>
    /// Holds the state of a physical seat
    /// </summary>
    [Serializable]
    public class PhysicalSeatState
    {
        public string SeatNumber { get; set; }
    }
}