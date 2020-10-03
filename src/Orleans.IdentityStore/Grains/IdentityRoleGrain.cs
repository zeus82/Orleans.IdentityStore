using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.IdentityStore.Grains
{
    public interface IIdentityRoleGrain<TRole> : IGrainWithGuidKey where TRole : IdentityRole<Guid>
    {
        Task AddUser(Guid id);

        Task<IdentityResult> Create(TRole role);

        Task<IdentityResult> Delete();

        [AlwaysInterleave]
        Task<TRole> Get();

        Task<IList<Guid>> GetUsers();

        Task RemoveUser(Guid id);

        Task<IdentityResult> Update(TRole role);
    }

    internal class IdentityRoleGrain<TRole> : Grain, IIdentityRoleGrain<TRole> where TRole : IdentityRole<Guid>
    {
        private readonly IPersistentState<RoleGrainState<TRole>> _data;
        private readonly ILogger<IdentityRoleGrain<TRole>> _logger;

        public IdentityRoleGrain(ILogger<IdentityRoleGrain<TRole>> logger,
            [PersistentState("IdentityRole", OrleansIdentityConstants.OrleansStorageProvider)] IPersistentState<RoleGrainState<TRole>> data)
        {
            _logger = logger;
            _data = data;
        }

        public Task AddUser(Guid id)
        {
            if (_data.State.Users.Add(id))
                return _data.WriteStateAsync();

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> Create(TRole role)
        {
            if (_data.State.Role != null)
                return IdentityResult.Failed();

            _data.State.Role = role;
            await GrainFactory.GetGrain<IIdentityRoleByNameGrain<TRole>>(role.NormalizedName).SetId(role.Id);
            await _data.WriteStateAsync();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> Delete()
        {
            if (_data.State.Role == null)
                return IdentityResult.Failed();

            await GrainFactory.GetGrain<IIdentityRoleByNameGrain<TRole>>(_data.State.Role.NormalizedName).ClearId();
            await _data.ClearStateAsync();

            return IdentityResult.Success;
        }

        public Task<TRole> Get()
        {
            return Task.FromResult(_data.State.Role);
        }

        public Task<IList<Guid>> GetUsers()
        {
            return Task.FromResult<IList<Guid>>(_data.State.Users.ToList());
        }

        public Task RemoveUser(Guid id)
        {
            if (_data.State.Users.Remove(id))
                return _data.WriteStateAsync();

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> Update(TRole role)
        {
            if (_data.State.Role == null)
                return IdentityResult.Failed();

            _data.State.Role = role;
            await _data.WriteStateAsync();

            return IdentityResult.Success;
        }
    }

    internal class RoleGrainState<TRole>
    {
        public TRole Role { get; set; }
        public HashSet<Guid> Users { get; set; } = new HashSet<Guid>();
    }
}