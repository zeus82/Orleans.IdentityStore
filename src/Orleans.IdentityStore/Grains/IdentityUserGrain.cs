using Microsoft.AspNetCore.Identity;
using Orleans.Concurrency;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        Task AddLogin(TUserLogin login);

        Task AddToRole(Guid roleId);

        [AlwaysInterleave]
        Task<bool> ContainsRole(Guid id);

        Task<IdentityResult> Create(TUser user);

        Task<IdentityResult> Delete();

        [AlwaysInterleave]
        Task<TUser> Get();

        Task<IList<TUserClaim>> GetClaims();

        [AlwaysInterleave]
        Task<IList<TUserLogin>> GetLogins();

        [AlwaysInterleave]
        Task<IList<string>> GetRoles();

        Task<TUserToken> GetToken(string loginProvider, string name);

        Task RemoveClaims(IList<Claim> claims);

        Task RemoveLogin(string loginProvider, string providerKey);

        Task RemoveRole(Guid id);

        Task ReplaceClaims(Claim claim, Claim newClaim);

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

        public Task AddClaims(IList<TUserClaim> claims)
        {
            throw new NotImplementedException();
        }

        public Task AddLogin(TUserLogin login)
        {
            throw new NotImplementedException();
        }

        public Task AddToRole(Guid roleId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ContainsRole(Guid id)
        {
            throw new NotImplementedException();
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

        public Task<IList<TUserClaim>> GetClaims()
        {
            throw new NotImplementedException();
        }

        public Task<IList<TUserLogin>> GetLogins()
        {
            throw new NotImplementedException();
        }

        public Task<ISet<string>> GetRoles()
        {
            throw new NotImplementedException();
        }

        public override Task OnActivateAsync()
        {
            _id = this.GetPrimaryKey();
            return Task.CompletedTask;
        }

        public Task RemoveClaims(IList<Claim> claims)
        {
            var writeRequired = false;
            foreach (var c in claims)
            {
                var matchedClaims = _data.State.Claims.Where(uc => uc.ClaimValue == c.Value && uc.ClaimType == c.Type);
                foreach (var m in matchedClaims)
                {
                    writeRequired = true;
                    _data.State.Claims.Remove(m);
                }
            }

            if (writeRequired)
                return _data.WriteStateAsync();

            return Task.CompletedTask;
        }

        public Task ReplaceClaims(Claim claim, Claim newClaim)
        {
            var matchedClaims = _data.State.Claims
                .Where(uc => uc.UserId.Equals(_id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type);

            if (matchedClaims.Any())
            {
                foreach (var c in matchedClaims)
                {
                    c.ClaimValue = newClaim.Value;
                    c.ClaimType = newClaim.Type;
                }

                return _data.WriteStateAsync();
            }

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
        public HashSet<Guid> Roles { get; set; }
        public List<TUserToken> Tokens { get; set; }
        public TUser User { get; set; }
    }
}