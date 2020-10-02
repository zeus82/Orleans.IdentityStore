using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.IdentityStore.Grains
{
    public interface IIdentityClaimGrain : IGrainWithStringKey
    {
        [AlwaysInterleave]
        Task<IList<Guid>> GetUserIds();
    }

    internal interface IIdentityClaimGrainInternal : IIdentityClaimGrain
    {
        Task AddUserId(Guid id);

        Task RemoveUserId(Guid id);
    }

    internal class IdentityClaimGrain : Grain, IIdentityClaimGrainInternal
    {
        private readonly IPersistentState<HashSet<Guid>> _data;
        private readonly ILogger<IdentityClaimGrain> _logger;

        public IdentityClaimGrain(ILogger<IdentityClaimGrain> logger,
                    [PersistentState("IdentityClaim", OrleansIdentityConstants.OrleansStorageProvider)] IPersistentState<HashSet<Guid>> data)
        {
            _logger = logger;
            _data = data;
        }

        public Task AddUserId(Guid id)
        {
            if (_data.State.Add(id))
                return _data.WriteStateAsync();

            return Task.CompletedTask;
        }

        public Task<IList<Guid>> GetUserIds()
        {
            return Task.FromResult<IList<Guid>>(_data.State.ToList());
        }

        public Task RemoveUserId(Guid id)
        {
            if (_data.State.Remove(id))
                return _data.WriteStateAsync();

            return Task.CompletedTask;
        }
    }
}
