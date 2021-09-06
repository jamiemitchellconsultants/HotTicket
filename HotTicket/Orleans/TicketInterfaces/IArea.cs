using Orleans;
using PublicTicketInterfaces;

namespace TicketInterfaces
{
    /// <summary>
    /// All methods are public in this case
    /// </summary>
    public interface IArea:IGrainWithStringKey,IPublicArea
    {

    }
}