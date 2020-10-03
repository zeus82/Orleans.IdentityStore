using Microsoft.AspNetCore.Identity;
using System;

namespace Orleans.IdentityStore.Tests
{
    public class TestRole : IdentityRole<Guid>
    {
    }

    public class TestUser : IdentityUser<Guid>
    {
    }
}