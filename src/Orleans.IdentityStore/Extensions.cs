using System.Security.Claims;

namespace Orleans.IdentityStore
{
    internal static class Extensions
    {
        public static string GetGrainKey(this Claim claim)
        {
            return $"{claim.Type}-{claim.Value}";
        }
    }
}
