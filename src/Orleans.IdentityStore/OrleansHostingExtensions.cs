using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Orleans.IdentityStore;
using Orleans.IdentityStore.Grains;
using Orleans.IdentityStore.Tools;
using System.Collections.Generic;

namespace Orleans.Hosting
{
    /// <summary>
    /// Silo hosting extensions
    /// </summary>
    public static class SiloBuilderExtensions
    {
        /// <summary>
        /// Add identity store to orleans. Grain storage provider name can be found at <see
        /// cref="OrleansIdentityConstants.OrleansStorageProvider"/> ///
        /// </summary>
        /// <param name="builder">Silo builder</param>
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
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IdentityByStringGrain).Assembly).WithReferences());
        }

        /// <summary>
        /// Add identity store to orleans. Grain storage provider name can be found at <see
        /// cref="OrleansIdentityConstants.OrleansStorageProvider"/> ///
        /// </summary>
        /// <param name="builder">Silo builder</param>
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
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IdentityByStringGrain).Assembly).WithReferences());
        }
    }
}