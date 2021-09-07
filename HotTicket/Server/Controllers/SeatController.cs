using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotTicket.Shared;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using PublicTicketInterfaces;
using TicketInterfaces;

namespace HotTicket.Server.Controllers
{
    /// <summary>
    /// Manage the seat in a performance
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SeatController : ControllerBase
    {
        private readonly IClusterClient _clusterClient;
        /// <summary>
        /// Need a connection to the orleans cluster
        /// </summary>
        /// <param name="clusterClient">Connection to the orleans cluster</param>
        public SeatController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;

        }
        /// <summary>
        /// Get the seats available for the performance
        /// </summary>
        /// <param name="performance">The name of the performance</param>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(List<SeatModel>))]
        [Route("performance/{performanceName}", Name = "GetPerformanceAvailableSeats")]
        public async Task<IActionResult> GetPerformanceAvailableSeats(string performance)
        {
            var performanceIndex = _clusterClient.GetGrain<IIndex<IPerformance>>("performance");
            var performanceGrain = await performanceIndex.GetItem(performance);
            if (performanceGrain == null)
            {
                return NotFound(performance);
            }

            var area = await performanceGrain.GetAreaName();
            if (!area.Success)
            {
                return NotFound(area.ErrorMessage);
            }

            var areaGrain = _clusterClient.GetGrain<IPublicArea>(area.Result);
            var seatsMessage = await areaGrain.GetAvailableSeats(performance);
            return Ok(seatsMessage.Result.Seats.Select(o => new SeatModel
            { Area = area.Result, Performance = performance, SeatId = o.SeatId.ToString() }));
        }


        /// <summary>
        /// Hold some seats for purchase
        /// </summary>
        /// <param name="seats">List fo seats to hold</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [Route("hold", Name = "CreateHold")]
        public async Task<IActionResult> CreateHold([FromBody] List<Guid> seats)
        {
            var seatGrain = _clusterClient.GetGrain<IPublicSeat>(seats[0]);
            var seatResponse = await seatGrain.GetSeat();
            if (!seatResponse.Success)
            {
                return Problem(seatResponse.ErrorMessage);
            }

            var seatData = seatResponse.Result;
            var areaGrain = _clusterClient.GetGrain<IPublicArea>(seatData.AreaName);

            var holdId = Guid.NewGuid();
            var holdGrain = _clusterClient.GetGrain<IPublicHold>(holdId);
            var holdResponse = await holdGrain.HoldSeats(seats);
            if (!holdResponse.Success)
            {
                return Problem(holdResponse.ErrorMessage);
            }

            var hold = holdResponse.Result;
            return CreatedAtAction(nameof(GetHold), new { id = hold.HoldId },
                new HoldModel { HoldId = hold.HoldId, SeatsHeld = hold.SeatsHeld });
        }

        /// <summary>
        /// Get the details of a hold
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("hold/{id:guid}", Name = "GetHold")]
        [Produces(typeof(HoldModel))]
        //[Route("hold")]
        public async Task<IActionResult> GetHold(Guid id)
        {
            var holdGrain = _clusterClient.GetGrain<IPublicHold>(id);
            var holdResponse = await holdGrain.GetHoldData();
            if (!holdResponse.Success)
            {
                return Problem(holdResponse.ErrorMessage);
            }

            var holdData = holdResponse.Result;
            return Ok(new HoldModel { HoldId = id, SeatsHeld = holdData.SeatsHeld });
        }
    }

}
