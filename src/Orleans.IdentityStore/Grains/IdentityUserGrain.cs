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
    public interface IIdentityUserGrain<TUser, TRole> : IGrainWithGuidKey
        where TUser : IdentityUser<Guid>
        where TRole : IdentityRole<Guid>
    {
        Task AddClaims(IList<IdentityUserClaim<Guid>> claims);

        Task AddLogin(IdentityUserLogin<Guid> login);

        Task AddToken(IdentityUserToken<Guid> token);

        Task AddToRole(Guid roleId);

        [AlwaysInterleave]
        Task<bool> ContainsRole(Guid id);

        Task<IdentityResult> Create(TUser user);

        Task<IdentityResult> Delete();

        [AlwaysInterleave]
        Task<TUser> Get();

        Task<IList<IdentityUserClaim<Guid>>> GetClaims();

        [AlwaysInterleave]
        Task<IdentityUserLogin<Guid>> GetLogin(string loginProvider, string providerKey);

        [AlwaysInterleave]
        Task<IList<IdentityUserLogin<Guid>>> GetLogins();

        [AlwaysInterleave]
        Task<IList<string>> GetRoles();

        [AlwaysInterleave]
        Task<IdentityUserToken<Guid>> GetToken(string loginProvider, string name);

        Task RemoveClaims(IList<Claim> claims);

        Task RemoveLogin(string loginProvider, string providerKey);

        Task RemoveRole(Guid id, bool updateRoleGrain);

        Task RemoveToken(IdentityUserToken<Guid> token);

        Task ReplaceClaims(Claim claim, Claim newClaim);

        Task<IdentityResult> Update(TUser user);
    }

    internal class IdentityUserGrain<TUser, TRole> :
        Grain, IIdentityUserGrain<TUser, TRole>
        where TUser : IdentityUser<Guid>, new()
        where TRole : IdentityRole<Guid>, new()
    {
        private readonly IPersistentState<IdentityUserGrainState<TUser, TRole>> _data;

        private Guid _id;

        public IdentityUserGrain(
                    [PersistentState("IdentityUser", OrleansIdentityConstants.OrleansStorageProvider)]
        IPersistentState<IdentityUserGrainState<TUser, TRole>> data)
        {
            _data = data;
        }

        private bool Exists => _data.State?.User != null;

        public Task AddClaims(IList<IdentityUserClaim<Guid>> claims)
        {
            if (Exists && claims?.Count > 0)
            {
                _data.State.Claims.AddRange(claims);
                return _data.WriteStateAsync();
            }

            return Task.CompletedTask;
        }

        public Task AddLogin(IdentityUserLogin<Guid> login)
        {
            if (Exists && login != null)
            {
                _data.State.Logins.Add(login);
                return _data.WriteStateAsync();
            }

            return Task.CompletedTask;
        }

        public Task AddToken(IdentityUserToken<Guid> token)
        {
            if (Exists && token != null)
            {
                _data.State.Tokens.Add(token);
                return _data.WriteStateAsync();
            }

            return Task.CompletedTask;
        }

        public Task AddToRole(Guid roleId)
        {
            if (Exists && _data.State.Roles.Add(roleId))
            {
                return _data.WriteStateAsync();
            }

            return Task.CompletedTask;
        }

        public Task<bool> ContainsRole(Guid id)
        {
            return Task.FromResult(Exists && _data.State.Roles.Contains(id));
        }

        public async Task<IdentityResult> Create(TUser user)
        {
            if (Exists || string.IsNullOrEmpty(user.NormalizedEmail) || string.IsNullOrEmpty(user.NormalizedUserName))
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
            await Task.WhenAll(_data.State.Roles.Select(r => GrainFactory.GetGrain<IIdentityRoleGrain<TUser, TRole>>(r).RemoveUser(_id)));
            await _data.ClearStateAsync();

            return IdentityResult.Success;
        }

        public Task<TUser> Get()
        {
            return Task.FromResult(_data.State.User);
        }

        public Task<IList<IdentityUserClaim<Guid>>> GetClaims()
        {
            if (Exists)
            {
                return Task.FromResult<IList<IdentityUserClaim<Guid>>>(_data.State.Claims);
            }
            return Task.FromResult<IList<IdentityUserClaim<Guid>>>(null);
        }

        public Task<IdentityUserLogin<Guid>> GetLogin(string loginProvider, string providerKey)
        {
            if (Exists)
            {
                return Task.FromResult(_data.State.Logins.Find(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey));
            }
            return Task.FromResult<IdentityUserLogin<Guid>>(null);
        }

        public Task<IList<IdentityUserLogin<Guid>>> GetLogins()
        {
            if (Exists)
            {
                return Task.FromResult<IList<IdentityUserLogin<Guid>>>(_data.State.Logins);
            }
            return Task.FromResult<IList<IdentityUserLogin<Guid>>>(null);
        }

        public async Task<IList<string>> GetRoles()
        {
            if (Exists)
            {
                return (await Task.WhenAll(_data.State.Roles.Select(r => GrainFactory.GetGrain<IIdentityRoleGrain<TUser, TRole>>(r).Get())))
                    .Select(r => r.Name)
                    .ToList();
            }

            return null;
        }

        public Task<IdentityUserToken<Guid>> GetToken(string loginProvider, string name)
        {
            if (Exists)
            {
                return Task.FromResult(_data.State.Tokens.Find(t => t.LoginProvider == loginProvider && t.Name == name));
            }

            return Task.FromResult<IdentityUserToken<Guid>>(null);
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
                foreach (var m in _data.State.Claims.Where(uc => uc.ClaimValue == c.Value && uc.ClaimType == c.Type))
                {
                    writeRequired = true;
                    _data.State.Claims.Remove(m);
                }
            }

            if (writeRequired)
                return _data.WriteStateAsync();

            return Task.CompletedTask;
        }

        public Task RemoveLogin(string loginProvider, string providerKey)
        {
            if (Exists)
            {
                var loginToRemove = _data.State.Logins.Find(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
                if (loginToRemove != null)
                {
                    _data.State.Logins.Remove(loginToRemove);
                    return _data.WriteStateAsync();
                }
            }

            return Task.CompletedTask;
        }

        public async Task RemoveRole(Guid id, bool updateRoleGrain)
        {
            if (Exists && _data.State.Roles.Remove(id))
            {
                if (updateRoleGrain)
                {
                    await GrainFactory.GetGrain<IIdentityRoleGrain<TUser, TRole>>(id).RemoveUser(_id);
                }

                await _data.WriteStateAsync();
            }
        }

        public Task RemoveToken(IdentityUserToken<Guid> token)
        {
            if (Exists)
            {
                var tokensToRemove = _data.State.Tokens.Find(t => t.LoginProvider == token.LoginProvider && t.Name == token.Name);
                if (tokensToRemove != null)
                {
                    _data.State.Tokens.Remove(tokensToRemove);
                    return _data.WriteStateAsync();
                }
            }

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

    internal class IdentityUserGrainState<TUser, TRole>
        where TUser : IdentityUser<Guid>, new()
        where TRole : IdentityRole<Guid>, new()
    {
        public List<IdentityUserClaim<Guid>> Claims { get; set; } = new List<IdentityUserClaim<Guid>>();
        public List<IdentityUserLogin<Guid>> Logins { get; set; } = new List<IdentityUserLogin<Guid>>();
        public HashSet<Guid> Roles { get; set; } = new HashSet<Guid>();
        public List<IdentityUserToken<Guid>> Tokens { get; set; } = new List<IdentityUserToken<Guid>>();
        public TUser User { get; set; }
    }
}