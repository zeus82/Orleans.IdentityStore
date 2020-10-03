using Microsoft.AspNetCore.Identity;
using Orleans.IdentityStore.Grains;
using System;
using System.Security.Claims;

namespace Orleans
{
    /// <summary>
    /// Grain Factory extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns the role grain
        /// </summary>
        /// <typeparam name="TUser">The user type</typeparam>
        /// <typeparam name="TRole">The role type</typeparam>
        /// <param name="factory">Grain factory</param>
        /// <param name="id">Role Id</param>
        public static IIdentityRoleGrain<TUser, TRole> Role<TUser, TRole>(this IGrainFactory factory, Guid id)
            where TUser : IdentityUser<Guid>
            where TRole : IdentityRole<Guid>
        {
            return factory.GetGrain<IIdentityRoleGrain<TUser, TRole>>(id);
        }

        /// <summary>
        /// Returns the role grain
        /// </summary>
        /// <typeparam name="TUser">The user type</typeparam>
        /// <param name="factory">Grain factory</param>
        /// <param name="id">Role Id</param>
        public static IIdentityRoleGrain<TUser, IdentityRole<Guid>> Role<TUser>(this IGrainFactory factory, Guid id)
            where TUser : IdentityUser<Guid>
        {
            return factory.GetGrain<IIdentityRoleGrain<TUser, IdentityRole<Guid>>>(id);
        }

        /// <summary>
        /// Returns the role grain
        /// </summary>
        /// <param name="factory">Grain factory</param>
        /// <param name="id">Role Id</param>
        public static IIdentityRoleGrain<IdentityUser<Guid>, IdentityRole<Guid>> Role(this IGrainFactory factory, Guid id)
        {
            return factory.GetGrain<IIdentityRoleGrain<IdentityUser<Guid>, IdentityRole<Guid>>>(id);
        }

        /// <summary>
        /// Returns the user grain
        /// </summary>
        /// <param name="factory">Grain factory</param>
        /// <param name="id">User Id</param>
        public static IIdentityUserGrain<IdentityUser<Guid>, IdentityRole<Guid>> User(this IGrainFactory factory, Guid id)
        {
            return factory.GetGrain<IIdentityUserGrain<IdentityUser<Guid>, IdentityRole<Guid>>>(id);
        }

        /// <summary>
        /// Returns the user grain
        /// </summary>
        /// <typeparam name="TUser">The user type</typeparam>
        /// <param name="factory">Grain factory</param>
        /// <param name="id">User Id</param>
        public static IIdentityUserGrain<TUser, IdentityRole<Guid>> User<TUser>(this IGrainFactory factory, Guid id)
            where TUser : IdentityUser<Guid>
        {
            return factory.GetGrain<IIdentityUserGrain<TUser, IdentityRole<Guid>>>(id);
        }

        /// <summary>
        /// Returns the user grain
        /// </summary>
        /// <typeparam name="TUser">The user type</typeparam>
        /// <typeparam name="TRole">The role type</typeparam>
        /// <param name="factory">Grain factory</param>
        /// <param name="id">User Id</param>
        public static IIdentityUserGrain<TUser, TRole> User<TUser, TRole>(this IGrainFactory factory, Guid id)
            where TUser : IdentityUser<Guid>
            where TRole : IdentityRole<Guid>
        {
            return factory.GetGrain<IIdentityUserGrain<TUser, TRole>>(id);
        }

        internal static IIdentityClaimGrainInternal GetGrain(this IGrainFactory factory, Claim claim)
        {
            return factory.GetGrain<IIdentityClaimGrainInternal>($"{claim.Type}-{claim.Value}");
        }

        internal static IIdentityClaimGrainInternal GetGrain(this IGrainFactory factory, IdentityUserClaim<Guid> claim)
        {
            return factory.GetGrain<IIdentityClaimGrainInternal>($"{claim.ClaimType}-{claim.ClaimValue}");
        }

        internal static IIdentityUserByLoginGrain GetGrain(this IGrainFactory factory, IdentityUserLogin<Guid> login)
        {
            return factory.GetGrain(login.LoginProvider, login.ProviderKey);
        }

        internal static IIdentityUserByLoginGrain GetGrain(this IGrainFactory factory, string loginProvider, string providerKey)
        {
            return factory.GetGrain<IIdentityUserByLoginGrain>($"{loginProvider}-{providerKey}");
        }
    }
}