using Microsoft.AspNetCore.Identity;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Based off of https://github.com/codekoenig/AspNetCore.Identity.DocumentDb
/// </summary>
namespace Orleans.IdentityStore.Grains
{
    public interface IIdentityUserGrain<TUser, TRole, TUserClaim, TUserLogin, TUserToken, TRoleClaim> : IGrainWithGuidKey
        where TUser : IdentityUser<Guid>
        where TRole : IdentityRole<Guid>
        where TUserClaim : IdentityUserClaim<Guid>, new()
        where TUserLogin : IdentityUserLogin<Guid>, new()
        where TUserToken : IdentityUserToken<Guid>, new()
        where TRoleClaim : IdentityRoleClaim<Guid>, new()
    {
        Task AddClaims(IList<TUserClaim> claims);

        Task<IdentityResult> Create(TUser user);

        Task<IdentityResult> Delete();

        [AlwaysInterleave]
        Task<TUser> Get();

        Task<IdentityResult> Update(TUser user);
    }

    internal class IdentityUserGrain<TUser, TRole, TUserClaim, TUserLogin, TUserToken, TRoleClaim> :
        Grain, IIdentityUserGrain<TUser, TRole, TUserClaim, TUserLogin, TUserToken, TRoleClaim>
        where TUser : IdentityUser<Guid>, new()
        where TRole : IdentityRole<Guid>, new()
        where TUserClaim : IdentityUserClaim<Guid>, new()
        where TUserLogin : IdentityUserLogin<Guid>, new()
        where TUserToken : IdentityUserToken<Guid>, new()
        where TRoleClaim : IdentityRoleClaim<Guid>, new()
    {
        private readonly IPersistentState<IdentityUserGrainState<TUser, TRole, TUserClaim, TUserLogin, TUserToken, TRoleClaim>> _data;

        private Guid _id;

        public IdentityUserGrain(
                    [PersistentState("IdentityUser", OrleansIdentityConstants.OrleansStorageProvider)]
        IPersistentState<IdentityUserGrainState<TUser, TRole, TUserClaim, TUserLogin, TUserToken, TRoleClaim>> data)
        {
            _data = data;
        }

        public async Task<IdentityResult> Create(TUser user)
        {
            if (_data.State.User != null || string.IsNullOrEmpty(user.NormalizedEmail) || string.IsNullOrEmpty(user.NormalizedUserName))
                return IdentityResult.Failed();

            _data.State.User = user;

            await GrainFactory.GetGrain<IIdentityUserByEmailGrain>(user.NormalizedEmail).SetId(user.Id);
            await GrainFactory.GetGrain<IIdentityUserByNameGrain>(user.NormalizedUserName).SetId(user.Id);
            //TODO
            //await GrainFactory.GetGrain<IIdentityUserByLoginGrain<TUser, TRole>>(user.Logins[0]).SetId(user.Id);
            await _data.WriteStateAsync();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> Delete()
        {
            if (_data.State.User == null)
                return IdentityResult.Failed();

            await GrainFactory.GetGrain<IIdentityUserByEmailGrain>(_data.State.User.NormalizedEmail).ClearId();
            await GrainFactory.GetGrain<IIdentityUserByNameGrain>(_data.State.User.NormalizedUserName).ClearId();
            await _data.ClearStateAsync();

            return IdentityResult.Success;
        }

        public Task<TUser> Get()
        {
            return Task.FromResult(_data.State.User);
        }

        public override Task OnActivateAsync()
        {
            _id = this.GetPrimaryKey();
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> Update(TUser user)
        {
            if (_data.State.User == null || user == null || string.IsNullOrEmpty(user.NormalizedEmail) || string.IsNullOrEmpty(user.NormalizedUserName))
                return IdentityResult.Failed();

            if (user.NormalizedEmail != _data.State.User.NormalizedEmail)
            {
                await GrainFactory.GetGrain<IIdentityUserByEmailGrain>(_data.State.User.NormalizedEmail).ClearId();
                await GrainFactory.GetGrain<IIdentityUserByEmailGrain>(user.NormalizedEmail).SetId(user.Id);
            }

            if (user.NormalizedUserName != _data.State.User.NormalizedUserName)
            {
                await GrainFactory.GetGrain<IIdentityUserByNameGrain>(_data.State.User.NormalizedUserName).ClearId();
                await GrainFactory.GetGrain<IIdentityUserByEmailGrain>(user.NormalizedEmail).SetId(user.Id);
            }

            _data.State.User = user;
            await _data.WriteStateAsync();

            return IdentityResult.Success;
        }
    }

    internal class IdentityUserGrainState<TUser, TRole, TUserClaim, TUserLogin, TUserToken, TRoleClaim>
        where TUser : IdentityUser<Guid>, new()
        where TRole : IdentityRole<Guid>, new()
        where TUserClaim : IdentityUserClaim<Guid>, new()
        where TUserLogin : IdentityUserLogin<Guid>, new()
        where TUserToken : IdentityUserToken<Guid>, new()
        where TRoleClaim : IdentityRoleClaim<Guid>, new()
    {
        public List<TUserClaim> Claims { get; set; }
        public List<TUserLogin> Logins { get; set; }
        public List<TRoleClaim> RoleClaims { get; set; }
        public List<Guid> Roles { get; set; }
        public List<TUserToken> Tokens { get; set; }
        public TUser User { get; set; }
    }
}