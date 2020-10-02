using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Orleans.IdentityStore
{
    public class OrleansIdentityRole
    {
        public OrleansIdentityRole()
        {
            Claims = new List<Claim>();
        }

        public IList<Claim> Claims { get; set; }
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string NormalizedName { get; set; }
    }
}
