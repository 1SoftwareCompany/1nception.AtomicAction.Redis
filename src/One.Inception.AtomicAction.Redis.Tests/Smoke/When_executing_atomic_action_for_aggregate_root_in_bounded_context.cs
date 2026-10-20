using System;
using System.Threading.Tasks;
using FakeItEasy;
using Machine.Specifications;
using One.Inception.AtomicAction.Redis.RevisionStore;
using One.Inception.Userfull;

namespace One.Inception.AtomicAction.Redis.Tests.Smoke;

[Subject("Redis Atomic Action")]
public class When_executing_atomic_action_for_aggregate_root_in_bounded_context
{
    Establish context = () =>
    {
        resource = new AtomicActionResource("volumes", new TestId());
        lockManager = A.Fake<ILock>();
        A.CallTo(() => lockManager.LockAsync(A<string>._, A<TimeSpan>._)).Returns(true);

        revisionStore = A.Fake<IRevisionStore>();
        A.CallTo(() => revisionStore.PrepareRevisionAsync(A<string>._, A<int>._)).Returns(new Result<int>(0));

        service = TestAtomicActionFactory.New(lockManager, revisionStore);
    };

    Because of = async () => result = await service.ExecuteAsync(resource, 1, () => Task.CompletedTask);

    It should_succeed = () => result.IsSuccessful.ShouldBeTrue();

    It should_lock_the_resource_scoped_by_bounded_context = () =>
        A.CallTo(() => lockManager.LockAsync("volumes:" + resource.AggregateRootId.Value, A<TimeSpan>._)).MustHaveHappened();

    It should_track_the_revision_of_the_resource_scoped_by_bounded_context = () =>
        A.CallTo(() => revisionStore.PrepareRevisionAsync("volumes:" + resource.AggregateRootId.Value, 1)).MustHaveHappened();

    It should_unlock_the_resource_scoped_by_bounded_context = () =>
        A.CallTo(() => lockManager.UnlockAsync("volumes:" + resource.AggregateRootId.Value)).MustHaveHappened();

    static AtomicActionResource resource;
    static ILock lockManager;
    static IRevisionStore revisionStore;
    static IAggregateRootAtomicAction service;
    static Result<bool> result;
}
