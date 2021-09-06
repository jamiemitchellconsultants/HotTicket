using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotTicket.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using PublicTicketInterfaces;
using TicketInterfaces;
using TicketMessages;

namespace HotTicket.Server.Controllers
{
    /// <summary>
    /// Controls management of tickets
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class TicketController : ControllerBase
    {
        private readonly IClusterClient _clusterClient;

        /// <summary>
        /// Constructor needs connection to silo
        /// </summary>
        /// <param name="clusterClient"></param>
        public TicketController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        /// <summary>
        /// Get Details about a ticket
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}", Name = "GetTicket")]
        [Produces(typeof(TicketModel))]
        public async Task<IActionResult> GetTicket(Guid id)
        {
            var ticketModel = new TicketModel();
            var ticketGrain = _clusterClient.GetGrain<IPublicTicket>(id);
            var detailsResponse=await ticketGrain.GetDetails();
            if (!detailsResponse.Success)
            {
                return NotFound();
            }

            ticketModel.AreaName = detailsResponse.Result.AreaName;
            ticketModel.PerformanceName = detailsResponse.Result.PerformanceName;
            ticketModel.PhysicalSeatNumber = detailsResponse.Result.SeatNumber;
            ticketModel.TicketId = id;
            ticketModel.EntryCode = detailsResponse.Result.EntryCode;
            return Ok(ticketModel);
        }

        /// <summary>
        /// Create a ticket
        /// </summary>
        /// <param name="sale">The seat or the hold with a list of seats</param>
        /// <returns>The seat or the hold with a list of seats</returns>
        [HttpPost(Name="CreateTicket")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateTicket([FromBody] SaleModel sale)
        {
            IPublicHold holdGrain;
            GrainResponse<TicketsMessage> saleResponse;
            if (sale.HoldId == null)
            {
                if (sale.SeatId == null)
                {
                    return Problem();
                }

                var holdId = Guid.NewGuid();
                holdGrain = _clusterClient.GetGrain<IPublicHold>(holdId);
                var holdResponse=await holdGrain.HoldSeat(sale.SeatId.Value);
                if (!holdResponse.Success)
                {
                    return Problem(); //should be resource not available
                }

                saleResponse = await holdGrain.Sell();
                return !saleResponse.Success ? Problem() : CreatedAtAction("GetTicket", new {id = saleResponse.Result.Tickets[0].TicketId});
                //ok so just got a seat id here
            }

            if (sale.SeatId != null)
            {
                return Problem();
            }

            

            holdGrain = _clusterClient.GetGrain<IPublicHold>(sale.HoldId.Value);
            saleResponse = await holdGrain.Sell();
            return !saleResponse.Success ? Problem() : CreatedAtAction("GetTickets", new {id = sale.HoldId.Value});
            //ok so just got a hold id here
        }
        /// <summary>
        /// Gets all the tickets for seats in a hold
        /// </summary>
        /// <param name="holdId"></param>
        /// <returns></returns>
        [HttpGet("hold/{id:guid}", Name = "GetTickets")]
        [Produces(typeof(List<TicketModel>))]
        public async Task<IActionResult> GetTickets(Guid holdId)
        {
            var holdGrain = _clusterClient.GetGrain<IPublicHold>(holdId);
            var holdResponse = await holdGrain.GetHoldData();
            if (!holdResponse.Success)
            {
                return Problem(holdResponse.ErrorMessage, "", StatusCodes.Status410Gone);
            }
            await Task.CompletedTask;
            return Problem();
        }
    }

}
