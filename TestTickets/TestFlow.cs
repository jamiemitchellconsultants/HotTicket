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

            var hold1Id = Guid.NewGuid();
            var hold1Grain = cluster.GrainFactory.GetGrain<IHold>(hold1Id);
            var hold2Id = Guid.NewGuid();
            var hold2Grain = cluster.GrainFactory.GetGrain<IHold>(hold2Id);
            var hold1List = new List<Guid>();
            var hold2List = new List<Guid>();
            //selecting a bunch of seats to hold. 
            for (int i = 0; i < 10; i++)
            {
                hold1List.Add(seatsMessage.Result.Seats[i].SeatId);
                hold1List.Add(seatsMessage.Result.Seats[i+10].SeatId);
                hold1List.Add(seatsMessage.Result.Seats[i+20].SeatId);
                hold1List.Add(seatsMessage.Result.Seats[i+30].SeatId);
                //mixing up the order for hold2 to create maximum chaos
                hold2List.Add(seatsMessage.Result.Seats[i + 20].SeatId);
                hold2List.Add(seatsMessage.Result.Seats[i + 40].SeatId);
                hold2List.Add(seatsMessage.Result.Seats[i + 10].SeatId);


            }

            var hold1Task = hold1Grain.HoldSeats(hold1List);
            var hold2Task = hold2Grain.HoldSeats(hold2List);

            Task.WaitAll(hold1Task, hold2Task);

            var hold1TaskResult = hold1Task.Result;
            var hold2TaskResult = hold2Task.Result;

            Assert.IsTrue(hold1TaskResult.Success);
            Assert.IsTrue(hold2TaskResult.Success);

            var totalSeats = hold2TaskResult.Result.SeatsHeld.Count + hold1TaskResult.Result.SeatsHeld.Count;

            //Add seats 0 to 40 to hold 1 and 10 to 29 + 40 to 49 to hold 2
            //the two holds cover 70 seats, 40 in hold 1 and 30 in hold 2
            //a total of 50 distinct seats with a 20 seat overlap
            //so no matter what order the seats were request to be held, the total held in hold 1 and hold 2 must be 50
            Assert.AreEqual(50,totalSeats);
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
