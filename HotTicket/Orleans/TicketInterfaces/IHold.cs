using Orleans;
using PublicTicketInterfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketMessages;

namespace TicketInterfaces
{
    public interface IHold:IGrainWithGuidKey,IPublicHold
    {

        //Task<GrainResponse> HoldSeat(ISeat seat);
        //Task<GrainResponse> HoldSeat(List<ISeat> seats);
    }
}