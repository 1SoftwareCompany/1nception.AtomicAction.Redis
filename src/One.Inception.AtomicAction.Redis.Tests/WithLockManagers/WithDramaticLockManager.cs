using System;
using One.Inception.AtomicAction.Redis.RevisionStore;
using FakeItEasy;
using Machine.Specifications;

namespace One.Inception.AtomicAction.Redis.Tests.WithLockManagers;

public abstract class WithDramaticLockManager
{
    Establish context = () =>
    {
        lockManager = A.Fake<ILock>();
        A.CallTo(() => lockManager.LockAsync(A<string>._, A<TimeSpan>._)).Throws(new Exception(message));
        service = TestAtomicActionFactory.New(lockManager, A.Fake<IRevisionStore>());
    };

    protected static ILock lockManager;
    protected static IAggregateRootAtomicAction service;
    protected static string message = "drama";
}
