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
            var builder = new TestClusterBuilder();
            builder.AddSiloBuilderConfigurator<TestSiloconfigurator>();
            var cluster = builder.Build();
            cluster.Deploy();

            var areaGrain = cluster.GrainFactory.GetGrain<IPublicArea>("Red");
            var resp = await areaGrain.InitialisePhysicalSeats(100);
            Assert.IsTrue(resp.Success);

            resp = await areaGrain.CreatePerformance("performance 1");
            Assert.IsTrue(resp.Success);

            var perfIndexGrain = cluster.GrainFactory.GetGrain<IIndex<IPerformance>>("performance");
            var perfResponse = await perfIndexGrain.GetItems("", IndexFilter.all);
            Assert.IsNotNull(perfResponse);
            var perfLength = perfResponse.Count;
            Assert.AreEqual(1,perfLength);

            resp = await areaGrain.CreatePerformance("performance 2");
            Assert.IsTrue(resp.Success);
            perfResponse = await perfIndexGrain.GetItems("", IndexFilter.all);
            Assert.IsNotNull(perfResponse);
            perfLength = perfResponse.Count;
            Assert.AreEqual(2, perfLength);
            var perfItem = perfResponse[1];
            var perfData = await perfItem.GetPerformanceData();
            Assert.IsTrue(perfData.Success);
            Assert.AreEqual("performance 2",perfData.Result.PerformanceName);
            Assert.AreEqual("Red", perfData.Result.AreaName);



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
