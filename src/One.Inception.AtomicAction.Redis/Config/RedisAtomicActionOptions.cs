using System;
using System.ComponentModel.DataAnnotations;
using Elders.RedLock;
using Microsoft.Extensions.Configuration;

namespace One.Inception.AtomicAction.Redis.Config;

public class RedisAtomicActionOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "The configuration `Inception:AtomicAction:Redis:ConnectionString` is required.")]
    public string ConnectionString { get; set; }

    /// <summary>
    /// The TTL which is applied in the beginning of the execution of the atomic action.
    /// By default it is 1 second.
    /// </summary>
    public TimeSpan LockTtl { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// This TTL is applied after a successful execution of the atomic action.
    /// The reason behind this decision is to make sure that there are no other nodes/threads which
    /// are executing an action against the specific AR + revision. If we do not do this there is a
    /// chance some other node/thread to overwrite our last action. By default this lock lasts for
    /// 5 seconds and it does not interrupt any other operations over the AR in normal action flow.
    /// </summary>
    public TimeSpan LongTtl { get; set; } = TimeSpan.FromSeconds(5);
}

internal class RedisAtomicActionOptionsProvider : InceptionOptionsProviderBase<RedisAtomicActionOptions>
{
    public RedisAtomicActionOptionsProvider(IConfiguration configuration) : base(configuration) { }

    public override void Configure(RedisAtomicActionOptions options)
    {
        configuration.GetSection("Inception:atomicaction:redis").Bind(options);
    }
}

internal class AtomicActionRedLockOptionsProvider : InceptionOptionsProviderBase<RedLockOptions>
{
    public AtomicActionRedLockOptionsProvider(IConfiguration configuration) : base(configuration) { }

    public override void Configure(RedLockOptions options)
    {
        configuration.GetSection("Inception:atomicaction:redis").Bind(options);
    }
}
