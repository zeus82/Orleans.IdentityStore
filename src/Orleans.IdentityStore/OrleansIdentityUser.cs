using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Orleans.IdentityStore
{
    public class OrleansIdentityUser : OrleansIdentityUser<OrleansIdentityRole>
    {
    }

    public class OrleansIdentityUser<TRole> where TRole : OrleansIdentityRole
    {
        public OrleansIdentityUser()
        {
            Roles = new List<TRole>();
            Logins = new List<UserLoginInfo>();
            Claims = new List<Claim>();
        }

        public int AccessFailedCount { get; set; }
        public string AuthenticatorKey { get; set; }
        public IList<Claim> Claims { get; set; }
        public string Email { get; set; }
        public Guid Id { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsPhoneNumberConfirmed { get; internal set; }
        public bool IsTwoFactorAuthEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEndDate { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string NormalizedEmail { get; set; }
        public string NormalizedUserName { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; internal set; }
        public IEnumerable<string> RecoveryCodes { get; set; }
        public IList<TRole> Roles { get; set; }
        public string SecurityStamp { get; set; }
        public string UserName { get; set; }
    }
}
