using Orleans.Runtime;

namespace Orleans.IdentityStore.Grains
{
    public interface IIdentityRoleByNameGrain : IIdentityByStringGrain
    {
    }

    public interface IIdentityUserByEmailGrain : IIdentityByStringGrain
    {
    }

    public interface IIdentityUserByLoginGrain : IIdentityByStringGrain
    {
    }

    public interface IIdentityUserByNameGrain : IIdentityByStringGrain
    {
    }

    internal class IdentityRoleByNameGrain : IdentityByStringGrain, IIdentityRoleByNameGrain
    {
        public IdentityRoleByNameGrain(
            [PersistentState("IdentityByString", OrleansIdentityConstants.OrleansStorageProvider)] IPersistentState<IdentityByStingState> data)
            : base(data)
        {
        }
    }

    internal class IdentityUserByEmailGrain : IdentityByStringGrain, IIdentityUserByEmailGrain
    {
        public IdentityUserByEmailGrain(
            [PersistentState("IdentityByString", OrleansIdentityConstants.OrleansStorageProvider)] IPersistentState<IdentityByStingState> data)
            : base(data)
        {
        }
    }

    internal class IdentityUserByLoginGrain : IdentityByStringGrain, IIdentityUserByLoginGrain
    {
        public IdentityUserByLoginGrain(
            [PersistentState("IdentityByString", OrleansIdentityConstants.OrleansStorageProvider)] IPersistentState<IdentityByStingState> data)
            : base(data)
        {
        }
    }

    internal class IdentityUserByNameGrain : IdentityByStringGrain, IIdentityUserByNameGrain
    {
        public IdentityUserByNameGrain(
            [PersistentState("IdentityByString", OrleansIdentityConstants.OrleansStorageProvider)] IPersistentState<IdentityByStingState> data)
            : base(data)
        {
        }
    }
}