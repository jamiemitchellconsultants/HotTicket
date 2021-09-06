using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using PublicTicketInterfaces;
using TicketInterfaces;
using TicketMessages;

namespace TicketGrains
{
    public class PerformanceGrain : Grain, IPerformance
    {
        private readonly IPersistentState<PerformanceState> _store;

        public PerformanceGrain([PersistentState("performance", "SeatStore")] IPersistentState<PerformanceState> store)
        {
            _store = store;
        }

        public async Task<GrainResponse> InitialisePerformance(IArea area, string performanceName, List<IPhysicalSeat> seats)
        {
            try
            {
                if (_store.State.Initialized)
                {
                    return GrainResponse.FailureResponse("Already initialized");
                }

                _store.State.Initialized = true;
                _store.State.AvailableSeats = new Dictionary<Guid, ISeat>();
                foreach (var physicalSeat in seats)
                {
                    var seat = GrainFactory.GetGrain<ISeat>(Guid.NewGuid());
                    await seat.InitializeSeat(area, this.AsReference<IPerformance>(), physicalSeat);
                    _store.State.AvailableSeats.Add(seat.GetPrimaryKey(), seat);
                }
                _store.State.Name = performanceName;
                _store.State.Area = area;
                await _store.WriteStateAsync();
                var performanceIndex = GrainFactory.GetGrain<IPublicIndex<IPerformance>>("performance");
                await performanceIndex.AddItem(this.AsReference<IPerformance>(), performanceName);
                return GrainResponse.SuccessResponse();
            }
            catch (Exception ex)
            {
                return GrainResponse.FailureResponse(ex.Message);
            }
        }



        public async Task<GrainResponse<List<ISeat>>> GetAvailableSeats()
        {
            return await Task.FromResult(GrainResponse<List<ISeat>>.SuccessResponse(_store.State.AvailableSeats.Values.ToList()));
        }
        public async Task<GrainResponse<string>> GetPerformanceName()
        {
            return await Task.FromResult(GrainResponse<string>.SuccessResponse(_store.State.Name));
        }
        public async Task<GrainResponse<string>> GetAreaName()
        {
            return await Task.FromResult(GrainResponse<string>.SuccessResponse(_store.State.Area.GetPrimaryKeyString()));
        }
        /// <summary>
        /// remove a seat from available seats list
        /// </summary>
        /// <param name="seat"></param>
        /// <returns></returns>
        public async Task<GrainResponse> MarkSeatNotAvailable(ISeat seat)
        {
            try
            {
                _store.State.AvailableSeats.Remove(seat.GetPrimaryKey());
                await _store.WriteStateAsync();
                return GrainResponse.SuccessResponse();
            }
            catch (Exception exception)
            {
                return GrainResponse.FailureResponse(exception.Message);
            }
        }

        public async Task<GrainResponse> MarkSeatAvailable(ISeat seat)
        {
            try
            {
                _store.State.AvailableSeats.Add(seat.GetPrimaryKey(), seat);
                await _store.WriteStateAsync();
                return GrainResponse.SuccessResponse();
            }
            catch (Exception exception)
            {
                return GrainResponse.FailureResponse(exception.Message);
            }
        }

        public async Task<GrainResponse<PerformanceData>> GetPerformanceData()
        {
            if (_store.State.Area == null) return GrainResponse<PerformanceData>.SuccessResponse(new PerformanceData { AreaName = "", PerformanceName = _store.State.Name });
            var areadata = await _store.State.Area.GetAreaData();
            return GrainResponse<PerformanceData>.SuccessResponse(new PerformanceData { AreaName= areadata.Result.AreaName, PerformanceName=_store.State.Name });
        }
    }
    [Serializable]
    public class PerformanceState
    {
        public bool Initialized { get; set; } = false;
        public IArea Area { get; set; }
        public string Name { get; set; } = "Not Initialized";
        public Dictionary<Guid, ISeat> AvailableSeats { get; set; } = new Dictionary<Guid, ISeat>();
    }
}