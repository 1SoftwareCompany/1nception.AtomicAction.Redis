using System;
using System.Threading.Tasks;
using One.AtomicAction;

namespace One.Inception.AtomicAction.Redis.AggregateRootLock;

public sealed class RedisAggregateRootLock : ILock
{
    private readonly IAtomicActionManager lockManager;

    public RedisAggregateRootLock(IAtomicActionManager lockManager)
    {
        if (lockManager is null) throw new ArgumentNullException(nameof(lockManager));

        this.lockManager = lockManager;
    }

    public Task<bool> IsLockedAsync(string resource)
    {
        if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));

        return lockManager.IsLockedAsync(resource);
    }

    public Task<bool> LockAsync(string resource, TimeSpan ttl)
    {
        if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));

        return lockManager.LockAsync(resource, ttl);
    }

    public Task UnlockAsync(string resource)
    {
        if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));

        return lockManager.UnlockAsync(resource);
    }
}
