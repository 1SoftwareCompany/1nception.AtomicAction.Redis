using System;
using FakeItEasy;
using Machine.Specifications;
using One.Inception.AtomicAction.Redis.RevisionStore;

namespace One.Inception.AtomicAction.Redis.Tests.WithLockManagers;

public abstract class WithLockManagerFailingToAcquireLock
{
    Establish context = async () =>
    {
        lockManager = A.Fake<ILock>();
        A.CallTo(() => lockManager.LockAsync(A<string>._, A<TimeSpan>._)).Returns(false);
        service = TestAtomicActionFactory.New(lockManager, A.Fake<IRevisionStore>());
    };

    protected static ILock lockManager;
    protected static IAggregateRootAtomicAction service;
}
