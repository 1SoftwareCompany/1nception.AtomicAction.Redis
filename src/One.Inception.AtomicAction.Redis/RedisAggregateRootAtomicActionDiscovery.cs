using System.Collections.Generic;
using One.Inception.AtomicAction.Redis.Config;
using One.Inception.AtomicAction.Redis.RevisionStore;
using One.Inception.Discoveries;
using Microsoft.Extensions.DependencyInjection;
using One.AtomicAction;

namespace One.Inception.AtomicAction.Redis;

public class RedisAggregateRootAtomicActionDiscovery : DiscoveryBase<IAggregateRootAtomicAction>
{
    protected override DiscoveryResult<IAggregateRootAtomicAction> DiscoverFromAssemblies(DiscoveryContext context)
    {
        return new DiscoveryResult<IAggregateRootAtomicAction>(GetModels(context), services => services
            .AddAtomicAction<AtomicActionRedLockOptionsProvider>()
            .AddOptions<RedisAtomicActionOptions, RedisAtomicActionOptionsProvider>());
    }

    IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
    {
        yield return new DiscoveredModel(typeof(IAggregateRootAtomicAction), typeof(RedisAggregateRootAtomicAction), ServiceLifetime.Singleton) { CanOverrideDefaults = true };
        yield return new DiscoveredModel(typeof(ILock), typeof(AggregateRootLock.RedisAggregateRootLock), ServiceLifetime.Singleton) { CanOverrideDefaults = true };
        yield return new DiscoveredModel(typeof(IRevisionStore), typeof(RedisRevisionStore), ServiceLifetime.Singleton);
    }
}
