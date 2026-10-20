using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using One.AtomicAction;
using One.Inception.AtomicAction.Redis.AggregateRootLock;
using One.Inception.AtomicAction.Redis.Config;
using One.Inception.AtomicAction.Redis.RevisionStore;

namespace One.Inception.AtomicAction.Redis.Integration.Tests;

/// <summary>
/// Services from different bounded contexts share the same redis and the same system aggregate root ids
/// (ex.: urn:tenant:projectionmanager:{contract}). They must not block each other.
/// </summary>
[TestFixture]
public class RedisAggregateRootAtomicActionTests
{
    AtomicActionManager lockManager;
    RedisRevisionStore revisionStore;
    RedisAggregateRootAtomicAction atomicAction;

    [SetUp]
    public void SetUp()
    {
        string connectionString = RedisFixture.Container.GetConnectionString();
        lockManager = new AtomicActionManager(new LockOptions(connectionString), NullLogger<AtomicActionManager>.Instance);
        revisionStore = new RedisRevisionStore(new AtomicActionOptions(connectionString), NullLogger<RedisRevisionStore>.Instance);
        atomicAction = new RedisAggregateRootAtomicAction(new RedisAggregateRootLock(lockManager), revisionStore, new AtomicActionOptions(connectionString));
    }

    [TearDown]
    public void TearDown()
    {
        atomicAction?.Dispose();
        lockManager?.Dispose();
        revisionStore?.Dispose();
    }

    [Test]
    public async Task SameAggregateRootIdInDifferentBoundedContextsDoesNotBlock()
    {
        var arId = new AggregateRootId("tenant", "projectionmanager", Guid.NewGuid().ToString("n"));
        var holdVolumes = new TaskCompletionSource();
        var volumesIsInside = new TaskCompletionSource();

        Task<Userfull.Result<bool>> volumes = atomicAction.ExecuteAsync(new AtomicActionResource("volumes", arId), 1, async () =>
        {
            volumesIsInside.SetResult();
            await holdVolumes.Task;
        });
        await volumesIsInside.Task;

        Userfull.Result<bool> relations = await atomicAction.ExecuteAsync(new AtomicActionResource("relations", arId), 1, () => Task.CompletedTask);
        holdVolumes.SetResult();

        Assert.That(relations.IsSuccessful, Is.True);
        Assert.That((await volumes).IsSuccessful, Is.True);
    }

    [Test]
    public async Task SameAggregateRootIdInTheSameBoundedContextIsBlocked()
    {
        var arId = new AggregateRootId("tenant", "projectionmanager", Guid.NewGuid().ToString("n"));
        var holdFirst = new TaskCompletionSource();
        var firstIsInside = new TaskCompletionSource();

        Task<Userfull.Result<bool>> first = atomicAction.ExecuteAsync(new AtomicActionResource("volumes", arId), 1, async () =>
        {
            firstIsInside.SetResult();
            await holdFirst.Task;
        });
        await firstIsInside.Task;

        Userfull.Result<bool> second = await atomicAction.ExecuteAsync(new AtomicActionResource("volumes", arId), 1, () => Task.CompletedTask);
        holdFirst.SetResult();

        Assert.That(second.IsSuccessful, Is.False);
        Assert.That((await first).IsSuccessful, Is.True);
    }
}

file sealed class LockOptions : IOptionsMonitor<One.AtomicAction.AtomicActionOptions>
{
    private readonly string connectionString;

    public LockOptions(string connectionString) => this.connectionString = connectionString;

    public One.AtomicAction.AtomicActionOptions CurrentValue => new() { ConnectionString = connectionString };

    public One.AtomicAction.AtomicActionOptions Get(string name) => CurrentValue;

    public IDisposable OnChange(Action<One.AtomicAction.AtomicActionOptions, string> listener) => null;
}

file sealed class AtomicActionOptions : IOptionsMonitor<RedisAtomicActionOptions>
{
    private readonly string connectionString;

    public AtomicActionOptions(string connectionString) => this.connectionString = connectionString;

    public RedisAtomicActionOptions CurrentValue => new() { ConnectionString = connectionString };

    public RedisAtomicActionOptions Get(string name) => CurrentValue;

    public IDisposable OnChange(Action<RedisAtomicActionOptions, string> listener) => null;
}
