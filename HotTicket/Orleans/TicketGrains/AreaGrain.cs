using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using TicketInterfaces;
using TicketMessages;

namespace TicketGrains
{
    /// <summary>
    /// An instance of an area. An area has physical seats and hosts performances
    /// </summary>
    public class AreaGrain : Grain, IArea
    {
        private readonly IPersistentState<AreaState> _store;
        public AreaGrain([PersistentState("area", "SeatStore")] IPersistentState<AreaState> store)
        {
            _store=store;
        }
        /// <summary>
        /// Create a new performance in the area
        /// </summary>
        /// <param name="performanceName">Name of the performance</param>
        /// <returns>Success/Failure</returns>
        public async Task<GrainResponse> CreatePerformance(string performanceName)
        {
            try
            {
                if (_store.State.PhysicalSeats==null) return GrainResponse.FailureResponse("Not Initialized");
                var performance = GrainFactory.GetGrain<IPerformance>(Guid.NewGuid());
                var performanceResponse= await performance.InitialisePerformance(this.AsReference<IArea>(), performanceName,
                    _store.State.PhysicalSeats);
                if (!performanceResponse.Success)
                {
                    return GrainResponse.FailureResponse(performanceResponse.ErrorMessage);
                }

                _store.State.Performances ??= new Dictionary<string, IPerformance>();
                _store.State.Performances.Add(performanceName, performance.AsReference<IPerformance>());
                await _store.WriteStateAsync();
                return GrainResponse.SuccessResponse();
            }
            catch (Exception ex)
            {
                return GrainResponse.FailureResponse(ex.Message);
            }
        }

        /// <summary>
        /// For a performance, get the seats that are available for holding/selling
        /// </summary>
        /// <param name="performanceName">Name of the performance to get seats</param>
        /// <returns></returns>
        public async Task<GrainResponse<AvailableSeatsMessage>> GetAvailableSeats(string performanceName)
        {
            try
            {
                if (_store.State.Performances == null || _store.State.Performances.Count == 0)
                {
                    return GrainResponse<AvailableSeatsMessage>.FailureResponse("No Performances");
                }

                if (!_store.State.Performances.ContainsKey(performanceName))
                {
                    return await Task.FromResult(
                        GrainResponse<AvailableSeatsMessage>.SuccessResponse(new AvailableSeatsMessage
                            {PerformanceNotFound = true}));
                }

                var available = new AvailableSeatsMessage
                {
                    PerformanceNotFound = false,
                    AreaName = _store.State.AreaName,
                    PerformanceName = performanceName,
                };
                var performance = _store.State.Performances[performanceName];
                var seatsResponse= await performance.GetAvailableSeats();
                if (!seatsResponse.Success)
                {
                    return GrainResponse<AvailableSeatsMessage>.FailureResponse(seatsResponse.ErrorMessage);
                }

                var physicalSeat =  seatsResponse.Result.Select(o=>o.GetInternalData());
                var seatData= await Task.WhenAll(physicalSeat);
                available.Seats= seatData.Select(o => new SeatAndPhysicalItem { PhysicalSeatId = o.Result.PhysicalSeatId, SeatId = o.Result.SeatId, PhysicalSeatNumber = o.Result.PhysicalSeatName}).ToList();
                return GrainResponse<AvailableSeatsMessage>.SuccessResponse(available);
            }
            catch (Exception ex)
            {
                return GrainResponse<AvailableSeatsMessage>.FailureResponse(ex.Message);
            }

        }

        /// <summary>
        /// List of performances set up in the area
        /// </summary>
        /// <returns>List of performances</returns>
        public async Task<GrainResponse< PerformanceList>> GetPerformanceList()
        {
            try
            {
                if (_store.State.Performances == null || _store.State.Performances.Count == 0)
                {
                    return GrainResponse<PerformanceList>.SuccessResponse(new PerformanceList()
                        {PerformanceNames = new List<string>()});
                }

                return await Task.FromResult(GrainResponse<PerformanceList>.SuccessResponse(new PerformanceList
                {
                    PerformanceNames = _store.State.Performances.Keys.ToList()
                }));
            }
            catch (Exception exception)
            {
                return GrainResponse<PerformanceList>.FailureResponse(exception.Message);
            }
        }

        /// <summary>
        /// Try to put a hold on seats in a performance
        /// </summary>
        /// <param name="performanceName">name of performance</param>
        /// <param name="seats">List of seats to hold</param>
        /// <returns>List of seats held/not held</returns>
        public async Task<GrainResponse< HeldSeatsResponse>> HoldSeats(string performanceName, List<Guid> seats)
        {
            try
            {
                if (_store.State.Performances == null)
                {
                    return GrainResponse<HeldSeatsResponse>.FailureResponse("No performances");
                }
                var performanceGrain = _store.State.Performances[performanceName];
                if (performanceGrain == null)
                {
                    return GrainResponse<HeldSeatsResponse>.FailureResponse("Performance not found in area");
                }
                var holdId = Guid.NewGuid();
                var holdGrain = GrainFactory.GetGrain<IHold>(holdId);
                var heldSeatsResponse = new HeldSeatsResponse { HoldId = holdId, SeatsHeld = new List<Guid>(), SeatsNotHeld = new List<Guid>() };
                foreach (var seat in seats)
                {
                    var seatGrain = GrainFactory.GetGrain<ISeat>(seat);
                    var holdResponse = await seatGrain.HoldSeat(holdGrain);
                    if (holdResponse.Success)
                    {
                        heldSeatsResponse.SeatsHeld.Add(seat);
                    }
                    else
                    {
                        heldSeatsResponse.SeatsNotHeld.Add(seat);
                    }
                }
                return GrainResponse<HeldSeatsResponse>.SuccessResponse(heldSeatsResponse);
            } catch (Exception ex)
            {
                return GrainResponse<HeldSeatsResponse>.FailureResponse(ex.Message);
            }
        }

        /// <summary>
        /// Like a constructor for the area
        /// </summary>
        /// <param name="seatCount" example="100">Number of seats to configure</param>
        /// <returns>Success/Failure response</returns>
        public async Task<GrainResponse> InitialisePhysicalSeats(int seatCount)
        {
            try
            {
                if (_store.State.PhysicalSeats != null && _store.State.PhysicalSeats.Count != 0)
                {
                    return GrainResponse.FailureResponse("Already Initialized");
                }

                _store.State.PhysicalSeats ??= new List<IPhysicalSeat>();
                for (var i = 0; i < seatCount; i++)
                {
                    var physicalSeat = GrainFactory.GetGrain<IPhysicalSeat>(Guid.NewGuid());
                    await physicalSeat.SetSeatNumber(i.ToString());
                    _store.State.PhysicalSeats.Add(physicalSeat);
                }

                await _store.WriteStateAsync();
            }
            catch (Exception ex)
            {
                return GrainResponse.FailureResponse(ex.Message);
            }

            return GrainResponse.SuccessResponse();
        }
        /// <summary>
        /// Sell held seats
        /// </summary>
        /// <param name="holdId"></param>
        /// <returns></returns>
         public async Task<GrainResponse< TicketsMessage>> SellSeat(Guid holdId)
         {
             return await Task.FromResult(GrainResponse<TicketsMessage>.FailureResponse("Not implemented"));
        }

        /// <summary>
        /// Return details about an area
        /// </summary>
        /// <returns></returns>
        public async Task<GrainResponse< AreaDetails>> GetAreaData()
        {
            return await Task.FromResult(GrainResponse<AreaDetails>.SuccessResponse(new AreaDetails
                {AreaName = this.GetPrimaryKeyString(), PhysicalSeatCount = _store.State.PhysicalSeats?.Count ?? 0}));
        }
    }
    /// <summary>
    /// Internal state of an area
    /// </summary>
    public class AreaState
    {
        /// <summary>
        /// Seats configured in an area
        /// </summary>
        public List<IPhysicalSeat> PhysicalSeats { get; set; }
        /// <summary>
        /// Performances set up
        /// </summary>
        public Dictionary<string,IPerformance> Performances { get; set; }
        /// <summary>
        /// Name of the area
        /// </summary>
        public string AreaName { get; set; } = "";
    }
}