using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotTicket.Shared;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using PublicTicketInterfaces;
using TicketInterfaces;
using TicketMessages;

namespace HotTicket.Server.Controllers
{
    /// <summary>
    /// Manage a match/performance
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PerformanceController : ControllerBase
    {
        private readonly IClusterClient _clusterClient;
        /// <summary>
        /// Need a connection to the orleans cluster
        /// </summary>
        /// <param name="clusterClient">Connection to the orleans cluster</param>
        public PerformanceController(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }
        /// <summary>
        /// Lists all 
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetPerformances")]
        [Produces(typeof(List<PerformanceModel>))]
        public async Task<IActionResult> GetPerformances()
        {
            var indexGrain = _clusterClient.GetGrain<IPublicIndex<IPerformance>>("performance");
            var performances = await indexGrain.GetItems("", IndexFilter.all);
            var result = new List<PerformanceModel>();
            var performanceTasks = new List<Task<GrainResponse<PerformanceData>>>();
            foreach (var performance in performances)
            {
                performanceTasks.Add(performance.GetPerformanceData());
            }
            Task.WaitAll(performanceTasks.ToArray());

            return Ok(performanceTasks.Select(o => new PerformanceModel { AreaName = o.Result.Result.AreaName, PerformanceName = o.Result.Result.PerformanceName }).ToList());
        }
        /// <summary>
        /// Gets details about a performance
        /// </summary>
        /// <param name="performanceName" example="The big cup match">The name of the performance required</param>
        /// <param name="areaName" example="Red">The name of the area</param>
        /// <returns>Performance details</returns>
        [HttpGet]
        [Produces(typeof(PerformanceModel))]
        [Route("{performanceName}/area/{areaName}", Name = "GetPerformance")]
        public async Task<IActionResult> GetPerformance(string performanceName, string areaName)
        {
            var areaGrain = _clusterClient.GetGrain<IPublicArea>(areaName);
            var performanceList = await areaGrain.GetPerformanceList();
            if (!performanceList.Success)
            {
                return Problem(performanceList.ErrorMessage);
            }

            var performance = performanceList.Result.PerformanceNames.FirstOrDefault(o => o == performanceName);
            return Ok(new PerformanceModel { AreaName = areaName, PerformanceName = performance ?? "_" });
        }

    }

}
