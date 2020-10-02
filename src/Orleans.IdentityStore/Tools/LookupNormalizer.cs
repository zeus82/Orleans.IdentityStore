using Microsoft.AspNetCore.Identity;

namespace Orleans.IdentityStore.Tools
{
    public class LookupNormalizer : ILookupNormalizer
    {
        public string Normalize(string key)
        {
            return key.Normalize().ToLowerInvariant();
        }

        public string NormalizeEmail(string key)
        {
            return key.Normalize().ToLowerInvariant();
        }

        public string NormalizeName(string key)
        {
            return key.Normalize().ToLowerInvariant();
        }
    }
}
