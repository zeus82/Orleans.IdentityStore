using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace Orleans.IdentityStore.Grains
{
    public interface IIdentityByStringGrain : IGrainWithStringKey
    {
        Task ClearId();

        [AlwaysInterleave]
        Task<Guid?> GetId();

        Task SetId(Guid id);
    }

    internal class IdentityByStingState
    {
        public Guid? Id { get; set; }
    }

    internal abstract class IdentityByStringGrain : Grain, IIdentityRoleByNameGrain, IIdentityUserByEmailGrain, IIdentityUserByLoginGrain, IIdentityUserByNameGrain

    {
        private readonly IPersistentState<IdentityByStingState> _data;

        protected IdentityByStringGrain(IPersistentState<IdentityByStingState> data)
        {
            _data = data;
        }

        public Task ClearId()
        {
            return _data.ClearStateAsync();
        }

        public Task<Guid?> GetId()
        {
            return Task.FromResult(_data.State.Id);
        }

        public Task SetId(Guid id)
        {
            if (_data.State.Id != null)
                throw new ArgumentException("The ID already exists");

            _data.State.Id = id;
            return _data.WriteStateAsync();
        }
    }
}