using One.Inception.AtomicAction.Redis.RevisionStore;

namespace One.Inception.AtomicAction.Redis.Tests;

public static class TestAtomicActionFactory
{
    public static IAggregateRootAtomicAction New(ILock aggregateRootLock, IRevisionStore revisionStore)
    {
        return new RedisAggregateRootAtomicAction(aggregateRootLock, revisionStore, new RedisAtomicActionOptionsMonitorMock());
    }
}
