using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using TicketInterfaces;
using System.Linq;
using PublicTicketInterfaces;
using TicketMessages;

namespace TicketGrains
{
    /// <summary>
    /// Do not use this if you have more than a couple of hundred items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IndexGrain<T>:Grain, IIndex<T> where T:IGrainWithGuidKey
    {
        private readonly IPersistentState<IndexState> _store;

        //this should be injected
        readonly Dictionary<IndexFilter, Func<string, string, bool>> comparisons = new()
        {
            {IndexFilter.eq,( a,  b)=>a==b  },
            {IndexFilter.gt,(a,b)=>string.Compare(a,b,StringComparison.InvariantCultureIgnoreCase)<0 },
            {IndexFilter.lt,(a,b)=>string.Compare(a,b, StringComparison.InvariantCultureIgnoreCase) >0 },
            {IndexFilter.inc, (a,b)=>a.Contains(b) },
            {IndexFilter.stw, (a,b)=>a.StartsWith(b) },
            {IndexFilter.enw, (a,b)=>a.EndsWith(b)  },
            {IndexFilter.all, (a,b)=>true }
        };

        public IndexGrain([PersistentState("index","SeatStore")]IPersistentState<IndexState> store)
        {
            _store = store;
        }
        public async Task AddItem(T item, string key)
        {
            _store.State.Items ??= new Dictionary<string, Guid>();
            _store.State.Items.Add(key,item.GetPrimaryKey());
            await _store.WriteStateAsync();
        }

        public async Task<T> GetItem(string key)
        {
            
            if (_store.State.Items == null) throw new InvalidOperationException("not initialized");
            return await Task.FromResult( GrainFactory.GetGrain<T>(_store.State.Items[key]));

        }

        public async Task<List<T>> GetItems(string filter,IndexFilter filterType=IndexFilter.all)
        {
            try
            {
                if (_store.State.Items == null) return new List<T>();
                return await Task.FromResult((from item in _store.State.Items where comparisons[filterType](item.Key, filter) select item.Value).Select(o => GrainFactory.GetGrain<T>(o)).ToList());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

    }

    [Serializable]
    public class IndexState
    {
        public Dictionary<string,Guid> Items { get; set; }
    }
}