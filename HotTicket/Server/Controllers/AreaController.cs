using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotTicket.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using PublicTicketInterfaces;

namespace HotTicket.Server.Controllers
{
    /// <summary>
    /// Manages the configuration of an area
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AreaController : ControllerBase
    {
        private readonly IClusterClient _clusterClient;
        /// <summary>
        /// Need a connection to the orleans cluster
        /// </summary>
        /// <param name="clusterClient">Connection to the orleans cluster</param>
        public AreaController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        /// <summary>
        /// Gets the available seats for a performance in the area
        /// </summary>
        /// <param name="areaName" example="Red">The area hosting the performance</param>
        /// <param name="performanceName" example="The Big Cup Final">Name of the event</param>
        /// <returns>Available seats</returns>
        [HttpGet]
        [Produces(typeof(List<SeatModel>))]
        [Route("{areaName}/performance/{performanceName}/seats", Name = "GetAvailableSeats")]
        public async Task<IActionResult> GetAvailableSeats(string areaName, string performanceName)
        {
            var areaGrain = _clusterClient.GetGrain<IPublicArea>(areaName);
            var seatsMessage = await areaGrain.GetAvailableSeats(performanceName);
            if (!seatsMessage.Success)
            {
                return Problem(seatsMessage.ErrorMessage);
            }
            return Ok(seatsMessage.Result.Seats.Select(o => new SeatModel { Area = areaName, Performance = performanceName, PhysicalSeatNumber = o.PhysicalSeatNumber, SeatId = o.SeatId.ToString(), PhysicalSeatId = o.PhysicalSeatId.ToString() }));
        }
        /// <summary>
        /// Creates and initializes an area
        /// </summary>
        /// <param name="createAreaRequest">Definition of the area to be initialized</param>
        /// <returns>Created action</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Route("", Name = "CreateArea")]
        public async Task<IActionResult> CreateArea([FromBody] AreaModel createAreaRequest)
        {
            var areaGrain = _clusterClient.GetGrain<IPublicArea>(createAreaRequest.AreaName);
            await areaGrain.InitialisePhysicalSeats(createAreaRequest.NumberOfSeats);
            var result = CreatedAtAction(nameof(GetArea), new { areaName = areaGrain.GetPrimaryKeyString() }, createAreaRequest);
            return result;
        }
        /// <summary>
        /// Gets the description of an area
        /// </summary>
        /// <param name="areaName" example="Red">The name of the area to be retrieved</param>
        /// <returns>Area details</returns>
        [HttpGet("{areaName}", Name = "GetArea")]
        [Produces(typeof(AreaModel))]
        //[Route("{areaName}", Name = "GetArea")]
        public async Task<IActionResult> GetArea(string areaName)
        {

            var areaGrain = _clusterClient.GetGrain<IPublicArea>(areaName);
            var areaData = await areaGrain.GetAreaData();
            if (!areaData.Success)
            {
                return NotFound();
            }

            var performances = await areaGrain.GetPerformanceList();
            if (!performances.Success)
            {
                return Problem(performances.ErrorMessage);
            }
            return Ok(new AreaModel { NumberOfSeats = areaData.Result.PhysicalSeatCount, AreaName = areaData.Result.AreaName, Performances = performances.Result.PerformanceNames });
        }
        /// <summary>
        /// Create a performance in an area
        /// </summary>
        /// <param name="areaName" example="Red">The area where the performance is created</param>
        /// <param name="createPerformanceRequest">Definition of the performance</param>
        /// <returns>Created Action</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Route("{areaName}/performance", Name = "CreatePerformance")]
        public async Task<IActionResult> CreatePerformance([FromRoute] string areaName, [FromBody] PerformanceModel createPerformanceRequest)
        {
            var areaGrain = _clusterClient.GetGrain<IPublicArea>(areaName);
            var createResponse = await areaGrain.CreatePerformance(createPerformanceRequest.PerformanceName);
            if (createResponse.Success)
            {
                return CreatedAtRoute("GetPerformance",
                    new { performanceName = createPerformanceRequest.PerformanceName, areaName },
                    createPerformanceRequest);
            }

            return Problem(createResponse.ErrorMessage);
        }


    }
}
