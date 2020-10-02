using Orleans;
using System;

namespace Orleans.IdentityStore.Stores
{
    public abstract class StoreBase : IDisposable
    {
        private bool disposed;

        protected StoreBase(IClusterClient cluster)
        {
            Cluster = cluster;
        }

        protected IClusterClient Cluster { get; }

        public void Dispose()
        {
            disposed = true;
        }

        protected virtual void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
