using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Orleans.IdentityStore.Stores;
using Orleans.IdentityStore.Tools;
using System.Collections.Generic;

namespace Orleans.IdentityStore
{
    public static class AspCoreHostingExtensions
    {
        public static IdentityBuilder AddOrleansStores(this IdentityBuilder builder)
        {
            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>() { new JsonClaimConverter(), new JsonClaimsPrincipalConverter(), new JsonClaimsIdentityConverter() }
                };
            };

            if (builder.RoleType != null)
            {
                builder.Services.AddTransient(
                    typeof(IRoleStore<>).MakeGenericType(builder.RoleType),
                    typeof(OrleansRoleStore<>).MakeGenericType(builder.RoleType));

                builder.Services.AddTransient(
                typeof(IUserStore<>).MakeGenericType(builder.UserType),
                typeof(OrleansUserStore<,>).MakeGenericType(builder.UserType, builder.RoleType));
            }
            else // no specified role
            {
                builder.Services.AddTransient<IRoleStore<OrleansIdentityRole>, OrleansRoleStore>();

                builder.Services.AddTransient(
                    typeof(IUserStore<>).MakeGenericType(builder.UserType),
                    typeof(OrleansUserStore<>).MakeGenericType(builder.UserType));
            }

            builder.Services.AddSingleton<ILookupNormalizer, LookupNormalizer>();

            return builder;
        }
    }
}