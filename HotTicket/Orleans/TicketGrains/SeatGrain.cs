using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using TicketInterfaces;
using TicketMessages;

namespace TicketGrains
{
    /// <summary>
    /// A sellable unit for a performance. A seat without a performance is an IPhysicalSeat
    /// </summary>
    public class SeatGrain:Grain,ISeat
    {
        private readonly IPersistentState<SeatState> _store;
        /// <summary>
        /// Pass in the persistence provider
        /// </summary>
        /// <param name="store"></param>
        public SeatGrain([PersistentState("seat", "SeatStore")] IPersistentState<SeatState> store)
        {
            _store = store;
        }
        /// <summary>
        /// Just like a constructor
        /// </summary>
        /// <param name="physicalSeat">Reference to teh physical seat</param>
        /// <returns></returns>
        public async Task<GrainResponse> InitializeSeat(IArea area, IPerformance performance, IPhysicalSeat physicalSeat)
        {
            if (_store.State.PhysicalSeat != null)
            {
                return GrainResponse.FailureResponse("Already Initialized");
            }
            _store.State.Area=area;
            _store.State.Performance=performance;
            _store.State.PhysicalSeat = physicalSeat;
            await _store.WriteStateAsync();
            return GrainResponse.SuccessResponse();
        }

        /// <summary>
        /// Hold a seat if it is free. Will use a timer (eventually) to release the seat
        /// </summary>
        /// <param name="hold">List of held seats this seat will be added to</param>
        /// <returns></returns>
        public async Task<GrainResponse> HoldSeat(IHold hold)
        {
            try
            {
                if (_store.State.Hold != null)
                {
                    return GrainResponse.FailureResponse("Seat Already Held");
                }

                if (_store.State.Ticket != null)
                {
                    return GrainResponse.FailureResponse("Seat Already Sold");
                }

                var holdResponse = await _store.State.Performance.MarkSeatNotAvailable(this.AsReference<ISeat>());
                if (holdResponse.Success)
                {
                    _store.State.Hold = hold;
                    await _store.WriteStateAsync();
                    return GrainResponse.SuccessResponse();
                }
                else
                {
                    return GrainResponse.FailureResponse("Cant hold seat");
                }
            }
            catch (Exception ex)
            {
                return GrainResponse.FailureResponse(ex.Message);
            }
        }
        /// <summary>
        /// Sell the seat if it is free (like a hold) 
        /// </summary>
        /// <returns></returns>
        public async Task<GrainResponse< ITicket>> SellSeat()
        {
            if (_store.State.Hold != null || _store.State.Ticket != null)
            {
                return  GrainResponse<ITicket>.FailureResponse("Sold or on hold");
            }
            //var holdResponse =
            await _store.State.Performance.MarkSeatNotAvailable(this.AsReference<ISeat>());

            var ticket = GrainFactory.GetGrain<ITicket>(Guid.NewGuid());
            var initResponse= await ticket.Initialize(this.AsReference<ISeat>());
            if (!initResponse.Success)
            {
                //gotta return seat to available
                return GrainResponse<ITicket>.FailureResponse(initResponse.ErrorMessage);
            }
            _store.State.Ticket = ticket;

            await _store.WriteStateAsync();
            return GrainResponse<ITicket>.SuccessResponse(ticket);
        }

        /// <summary>
        /// Sell a seat that has been held
        /// </summary>
        /// <param name="hold">An existing hold lists the seats being sold</param>
        /// <returns></returns>
        public async Task<GrainResponse<ITicket>> SellSeat(IHold hold)
        {
            if (_store.State.Ticket!=null || _store.State.Hold == null || _store.State.Hold.GetPrimaryKey() != hold.GetPrimaryKey()) return GrainResponse<ITicket>.FailureResponse("Seat not in this hold"); ;
            var ticket = GrainFactory.GetGrain<ITicket>(Guid.NewGuid());
            var initResponse = await ticket.Initialize(this.AsReference<ISeat>());
            if (!initResponse.Success)
            {
                return GrainResponse<ITicket>.FailureResponse(initResponse.ErrorMessage);
            }
            _store.State.Ticket = ticket;
            await _store.WriteStateAsync();
            return GrainResponse<ITicket>.SuccessResponse(ticket);

        }

        public async Task<GrainResponse<InternalSeatData>> GetInternalData(bool includeTicket=false)
        {
            string physicalSeatNumber = "";
            if (_store.State.PhysicalSeat != null)
            {
                var physicalSeatNumberResponse = await _store.State.PhysicalSeat.GetSeatNumber();
                if (physicalSeatNumberResponse.Success)
                {
                    physicalSeatNumber = physicalSeatNumberResponse.Result;
                }
            }

            TicketDetails ticketDetails = default;
            if (includeTicket)
            {
                var ticketDetailsResponse = await _store.State.Ticket.GetDetails();
                if (ticketDetailsResponse.Success)
                {
                    ticketDetails = ticketDetailsResponse.Result;
                }
            }

            return await Task.FromResult(GrainResponse<InternalSeatData>.SuccessResponse(
                new InternalSeatData
                {
                    SeatId = this.GetPrimaryKey(), 
                    PhysicalSeatId = _store.State.PhysicalSeat.GetPrimaryKey(),
                    PhysicalSeatName = physicalSeatNumber,
                    TicketDetails = ticketDetails
                }));
        }
        public async Task<GrainResponse<IPerformance>> GetPerformance()
        {
            return await Task.FromResult(GrainResponse<IPerformance>.SuccessResponse(_store.State.Performance));
        }

        /// <summary> 
        /// Get data about a seat
        /// </summary>
        /// <returns>The area, performance and physical seat</returns>
        public async Task<GrainResponse< SeatData>> GetSeat()
        {
            if (_store.State.PhysicalSeat == null)
                return GrainResponse<SeatData>.FailureResponse("No physical seat in Sate");
            var areaResponse = await _store.State.Area.GetAreaData();
            if(!areaResponse.Success)
                return GrainResponse<SeatData>.FailureResponse(areaResponse.ErrorMessage);
            var area = areaResponse.Result;
            var performanceResponse = await _store.State.Performance.GetPerformanceName();
            var performance = performanceResponse.Result;
            var physicalResponse = await _store.State.PhysicalSeat.GetSeatNumber();
            if(!physicalResponse.Success)
                return GrainResponse<SeatData>.FailureResponse(physicalResponse.ErrorMessage);
            var physical = physicalResponse.Result;
            return GrainResponse<SeatData>.SuccessResponse( new SeatData { PhysicalSeat = physical, AreaName = area.AreaName, PerformanceName = performance });
        }

        public async Task<GrainResponse> ReleaseHold(IHold hold)
        {
            _store.State.Hold = null;
            await _store.WriteStateAsync();
            return GrainResponse.SuccessResponse();
        }
    }

    /// <summary>
    /// Internal state of the seat
    /// </summary>
    [Serializable]
    public class SeatState
    {
        public IArea Area { get; set; }
        public IPerformance Performance{get;set;}
        public IPhysicalSeat PhysicalSeat { get; set; }
        public IHold Hold { get; set; }
        public ITicket Ticket { get; set; }
    }
}