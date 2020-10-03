using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Orleans.IdentityStore.Stores;
using Orleans.IdentityStore.Tools;
using System;
using System.Collections.Generic;

namespace Orleans.IdentityStore
{
    public static class AspCoreHostingExtensions
    {
        public static IdentityBuilder AddOrleansStores<TUser, TRole>(this IdentityBuilder builder)
            where TUser : IdentityUser<Guid>
            where TRole : IdentityRole<Guid>
        {
            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>() { new JsonClaimConverter(), new JsonClaimsPrincipalConverter(), new JsonClaimsIdentityConverter() }
                };
            };

            var roleType = builder.RoleType ?? typeof(IdentityRole<Guid>);

            builder.Services.AddTransient(
                    typeof(IRoleClaimStore<>).MakeGenericType(roleType),
                    typeof(OrleansRoleStore<,>).MakeGenericType(builder.UserType, roleType));

            builder.Services.AddTransient(
            typeof(IUserStore<>).MakeGenericType(builder.UserType),
            typeof(OrleansUserStore<,>).MakeGenericType(builder.UserType, roleType));

            builder.Services.AddSingleton<ILookupNormalizer, LookupNormalizer>();
            //builder.AddSignInManager();

            return builder;
        }
    }
}