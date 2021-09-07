using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Orleans.Hosting;
using Orleans.TestingHost;
using PublicTicketInterfaces;
using TicketInterfaces;
using TicketMessages;

namespace TestTickets
{
    public class TestFlow
    {
        [Test]
        public async Task Flow1Test()
        {
            //set up an in mem cluster
            var builder = new TestClusterBuilder();
            builder.AddSiloBuilderConfigurator<TestSiloconfigurator>();
            var cluster = builder.Build();
            cluster.Deploy();


            //create an area
            var areaGrain = cluster.GrainFactory.GetGrain<IPublicArea>("Red");
            var resp = await areaGrain.InitialisePhysicalSeats(100);
            Assert.IsTrue(resp.Success);

            //create a performance
            resp = await areaGrain.CreatePerformance("performance 1");
            Assert.IsTrue(resp.Success);

            //check the performance is in the index
            var perfIndexGrain = cluster.GrainFactory.GetGrain<IIndex<IPerformance>>("performance");
            var perfResponse = await perfIndexGrain.GetItems("", IndexFilter.all);
            Assert.IsNotNull(perfResponse);
            var perfLength = perfResponse.Count;
            Assert.AreEqual(1,perfLength);

            //create another performance
            resp = await areaGrain.CreatePerformance("performance 2");
            Assert.IsTrue(resp.Success);
            //and check the index is updated 
            perfResponse = await perfIndexGrain.GetItems("", IndexFilter.all);
            Assert.IsNotNull(perfResponse);
            perfLength = perfResponse.Count;
            Assert.AreEqual(2, perfLength);
            var perfItem = perfResponse[1];
            var perfData = await perfItem.GetPerformanceData();
            Assert.IsTrue(perfData.Success);
            Assert.AreEqual("performance 2",perfData.Result.PerformanceName);
            Assert.AreEqual("Red", perfData.Result.AreaName);

            //check the seats in the performance
            var seatsMessage = await areaGrain.GetAvailableSeats("performance 2");
            Assert.IsTrue(seatsMessage.Success);
            Assert.AreEqual(100,seatsMessage.Result.Seats.Count);



        }

    }

    public class TestSiloconfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddMemoryGrainStorage("SeatStore");

        }
    }
}
