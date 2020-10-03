using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace Orleans.IdentityStore.Grains
{
    public interface IIdentityByStringGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Clears the user ID associated with given string
        /// </summary>
        Task ClearId();

        /// <summary>
        /// Returns the user ID associated with the string
        /// </summary>
        [AlwaysInterleave]
        Task<Guid?> GetId();

        /// <summary>
        /// Sets the user ID associated with given string
        /// </summary>
        /// <param name="id">The user Id</param>
        Task SetId(Guid id);
    }

    public interface IIdentityRoleByNameGrain : IIdentityByStringGrain
    {
    }

    public interface IIdentityUserByEmailGrain : IIdentityByStringGrain
    {
    }

    public interface IIdentityUserByLoginGrain : IIdentityByStringGrain
    {
    }

    public interface IIdentityUserByNameGrain : IIdentityByStringGrain
    {
    }

    internal class IdentityByStingState
    {
        public Guid? Id { get; set; }
    }

    internal class IdentityByStringGrain : Grain, IIdentityRoleByNameGrain, IIdentityUserByEmailGrain, IIdentityUserByLoginGrain, IIdentityUserByNameGrain

    {
        private readonly IPersistentState<IdentityByStingState> _data;

        public IdentityByStringGrain(
            [PersistentState("IdentityByString", OrleansIdentityConstants.OrleansStorageProvider)] IPersistentState<IdentityByStingState> data)
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