using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Orleans.Hosting;
using Orleans.IdentityStore.Grains;
using Orleans.IdentityStore.Tools;
using System.Collections.Generic;

namespace Orleans.IdentityStore
{
    public static class SiloBuilderExtensions
    {
        public static ISiloBuilder UseOrleanIdentityStore(this ISiloBuilder builder)
        {
            builder.ConfigureServices(s => s.AddSingleton<ILookupNormalizer, LookupNormalizer>());
            try { builder.AddMemoryGrainStorage(OrleansIdentityConstants.OrleansStorageProvider); }
            catch { /** PubSubStore was already added. Do nothing. **/ }

            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>() { new JsonClaimConverter(), new JsonClaimsPrincipalConverter(), new JsonClaimsIdentityConverter() }
                };
            };

            return builder
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IdentityClaimGrain).Assembly).WithReferences());
        }
    }

    public static class SiloHostBuilderExtensions
    {
        public static ISiloHostBuilder UseOrleanIdentityStore(this ISiloHostBuilder builder)
        {
            try { builder.AddMemoryGrainStorage(OrleansIdentityConstants.OrleansStorageProvider); }
            catch { /** Grain storage provider was already added. Do nothing. **/ }

            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>() { new JsonClaimConverter(), new JsonClaimsPrincipalConverter(), new JsonClaimsIdentityConverter() }
                };
            };

            return builder
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IdentityClaimGrain).Assembly).WithReferences());
        }
    }
}