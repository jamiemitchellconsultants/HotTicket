using Orleans;
using PublicTicketInterfaces;

namespace TicketInterfaces
{
    public interface IIndex<T>:IPublicIndex<T>,IGrainWithStringKey where T:IGrainWithGuidKey
    {
 
    }
}