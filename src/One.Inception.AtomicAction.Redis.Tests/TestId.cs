using System;

namespace One.Inception.AtomicAction.Redis.Tests;

public class TestId : AggregateRootId
{
    public TestId() : this(Guid.NewGuid()) { }
    public TestId(Guid id) : base("testtenant", "redis-test", id.ToString()) { }
}
