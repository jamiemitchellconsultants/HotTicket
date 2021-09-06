using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotTicket.Shared
{
    /// <summary>
    /// Representation of a ticket
    /// </summary>
    public class TicketModel
    {
        /// <summary>
        /// Id of the ticket
        /// </summary>
        public Guid TicketId { get; set; }
        /// <summary>
        /// Entry code on the ticket. The idea is that entry code cant retrive details , just a validation
        /// </summary>
        public string EntryCode { get; set; }

        /// <summary>
        /// Area the ticket is for
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// Performance the ticket is for
        /// </summary>
        public string PerformanceName { get; set; }
        /// <summary>
        /// The seat the ticket is for
        /// </summary>
        public string PhysicalSeatNumber { get; set; }


    }
}
