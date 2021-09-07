using Orleans;
using Orleans.Runtime;
using PublicTicketInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketInterfaces;
using TicketMessages;

namespace TicketGrains
{
    public class HoldGrain:Grain,IHold
    {
        private readonly IPersistentState<HoldState> _store;

        public HoldGrain([PersistentState("hold", "SeatStore")] IPersistentState<HoldState> store)
        {
            _store = store;
        }

        public async Task<GrainResponse<HoldResponse>> GetHoldData(bool includePhysicalSeat=false, bool includeTicket=false)
        {
            var resp = new HoldResponse { HoldId = this.GetPrimaryKey(), SeatsHeld = _store.State.SeatsInHold.Select(o=>o.GetPrimaryKey()).ToList() };
            if (includePhysicalSeat)
            {
                resp.SeatData = new List<SeatAndPhysicalItem>();
                foreach (var seat in _store.State.SeatsInHold)
                {
                    var physSeatResp = await seat.GetInternalData(false);
                    if (physSeatResp.Success)
                    {
                        resp.SeatData.Add(new SeatAndPhysicalItem
                        {
                            PhysicalSeatId = physSeatResp.Result.PhysicalSeatId,
                            PhysicalSeatNumber = physSeatResp.Result.PhysicalSeatName,
                            SeatId = physSeatResp.Result.SeatId
                        });
                    }

                }

            }

            if (includeTicket)
            {
                var performanceResp =  await _store.State.Performance.GetPerformanceData();

                var ticketsMessage = new TicketsMessage
                {
                    AreaName = performanceResp.Result.AreaName,
                    PerformanceName = performanceResp.Result.PerformanceName
                };
                var ticketsList = new List<TicketDetails>();
                ticketsMessage.Tickets = ticketsList;
                foreach (var seat in _store.State.SeatsInHold)
                {
                    var seatresp = await seat.GetInternalData(true);
                    var ticketDetails= seatresp.Result.TicketDetails;
                    ticketsList.Add(ticketDetails);
                }
                resp.TicketsMessage = ticketsMessage;
            }
            return await Task.FromResult( GrainResponse<HoldResponse>.SuccessResponse(resp));
        }


        //public async Task<GrainResponse> HoldSeat(ISeat seat)
        //{
        //    _store.State.SeatsInHold.Add(seat);
        //    await _store.WriteStateAsync();
        //    return GrainResponse.SuccessResponse();
        //}

        //public async Task<GrainResponse> HoldSeat(List<ISeat> seats)
        //{
        //    _store.State.SeatsInHold.AddRange(seats);
        //    await _store.WriteStateAsync();
        //    return GrainResponse.SuccessResponse();
        //}

        public async Task<GrainResponse<HoldResponse>> HoldSeat(Guid seat)
        {
            try
            {
                var seatGrain = GrainFactory.GetGrain<ISeat>(seat);
                if (_store.State.Performance == null)
                    _store.State.Performance = (await seatGrain.GetPerformance()).Result;


                var seatResponse = await seatGrain.HoldSeat(this.AsReference<IHold>());
                if (seatResponse.Success)
                {
                    var notAvailableResponse= await _store.State.Performance.MarkSeatNotAvailable(seatGrain);
                    if (notAvailableResponse.Success)
                    {
                        _store.State.SeatsInHold.Add(seatGrain);
                        await _store.WriteStateAsync();
                        return GrainResponse<HoldResponse>.SuccessResponse(new HoldResponse { HoldId = this.GetPrimaryKey(), SeatsHeld = _store.State.SeatsInHold.Select(o => o.GetPrimaryKey()).ToList() });
                    }
                    GrainResponse release= await seatGrain.ReleaseHold(this.AsReference<IHold>());
                    return GrainResponse<HoldResponse>.FailureResponse("Can not hold seat");

                }
                return GrainResponse<HoldResponse>.FailureResponse(seatResponse.ErrorMessage);
            } catch (Exception ex)
            {
                return GrainResponse<HoldResponse>.FailureResponse(ex.Message);
            }

        }

        public async Task<GrainResponse<HoldResponse>> HoldSeats(List<Guid> seats)
        {
            var heldSeats = new List<ISeat>();
            try
            {
                var seatOuterGrain = GrainFactory.GetGrain<ISeat>(seats[0]);
                if (_store.State.Performance == null)
                    _store.State.Performance = (await seatOuterGrain.GetPerformance()).Result;
                foreach (var seat in seats)
                {
                    var seatGrain = GrainFactory.GetGrain<ISeat>(seat);
                    
                    var seatResponse =await seatGrain.HoldSeat(this.AsReference<IHold>());
                    if (seatResponse.Success)
                    {
                        heldSeats.Add(seatGrain);
                        var notAvailableResponse = await _store.State.Performance.MarkSeatNotAvailable(seatGrain);
                        if (notAvailableResponse.Success)
                        {
                            _store.State.SeatsInHold.Add(seatGrain);
                        } else
                        {
                            GrainResponse release = await seatGrain.ReleaseHold(this.AsReference<IHold>());
                        }
                    }
                }
                await _store.WriteStateAsync();
                return GrainResponse<HoldResponse>.SuccessResponse(new HoldResponse { HoldId = this.GetPrimaryKey(), SeatsHeld = heldSeats.Select(o => o.GetPrimaryKey()).ToList() });
            }
            catch (Exception ex)
            {
                if (_store.State.Performance != null)
                {
                    var TaskList = new List<Task>();
                    foreach (ISeat seat in heldSeats)
                    {
                        TaskList.Add(seat.ReleaseHold(this.AsReference<IHold>()));
                        TaskList.Add(_store.State.Performance.MarkSeatAvailable(seat));
                    }
                    await Task.WhenAll(TaskList);
                }
                return GrainResponse<HoldResponse>.FailureResponse(ex.Message);
            }
        }



        public async Task<GrainResponse<HoldResponse>> Release()
        {
            return await Task.FromResult(GrainResponse<HoldResponse>.FailureResponse("Not Implemented"));
        }

        public async Task<GrainResponse<TicketsMessage>> Sell()
        {
            //go through each seat in the hold and create a ticket, linked to the seat.
            var ticketList = new List<TicketDetails>();
            var areaName = "";
            var performanceName = "";
            try
            {
                foreach (var seat in _store.State.SeatsInHold)
                {
                    
                    var ticketResponse = await seat.SellSeat(this.AsReference<IHold>());
                    if (ticketResponse.Success)
                    {
                        var ticketMessage =await  ticketResponse.Result.GetDetails();
                        if (areaName =="") areaName = ticketMessage.Result.AreaName;
                        if (performanceName == "") performanceName = ticketMessage.Result.PerformanceName;
                        ticketList.Add( new TicketDetails
                        {
                            EntryCode = ticketMessage.Result.EntryCode,
                            SeatNumber = ticketMessage.Result.SeatNumber,
                            TicketId = ticketMessage.Result.TicketId
                        });
                    }
                }

                var ticketsMessage = new TicketsMessage
                {
                    AreaName = areaName,
                    PerformanceName = performanceName,
                    Tickets = ticketList
                };
                return await Task.FromResult(GrainResponse<TicketsMessage>.SuccessResponse(ticketsMessage));
            }
            catch (Exception ex)
            {
                return GrainResponse<TicketsMessage>.FailureResponse(ex.Message);
            }
            finally
            {

            }
        }
    }

    public class HoldState
    { 
        public IPerformance Performance { get; set; }
        public List<ISeat> SeatsInHold { get; set; }=new List<ISeat>();
    }
}