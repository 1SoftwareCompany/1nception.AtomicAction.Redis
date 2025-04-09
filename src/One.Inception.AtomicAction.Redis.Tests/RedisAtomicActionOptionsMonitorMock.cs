using System;
using One.Inception.AtomicAction.Redis.Config;
using Microsoft.Extensions.Options;

namespace One.Inception.AtomicAction.Redis.Tests;

public class RedisAtomicActionOptionsMonitorMock : IOptionsMonitor<RedisAtomicActionOptions>
{
    public RedisAtomicActionOptions CurrentValue => new RedisAtomicActionOptions();

    public RedisAtomicActionOptions Get(string name)
    {
        return CurrentValue;
    }

    public IDisposable OnChange(Action<RedisAtomicActionOptions, string> listener)
    {
        return null;
    }
}
