using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using TicketMessages;

namespace PublicTicketInterfaces
{
    public interface IPublicIndex<T>:IGrainWithStringKey where T:IGrainWithGuidKey
    {
        Task AddItem(T item, string key);
        Task<T> GetItem(string key);
        Task<List<T>> GetItems(string filter,IndexFilter indexFilter);
    }
}