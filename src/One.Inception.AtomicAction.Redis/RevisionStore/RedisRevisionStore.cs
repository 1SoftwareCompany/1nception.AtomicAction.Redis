﻿using System;
using System.Linq;
using System.Threading.Tasks;
using One.Inception.AtomicAction.Redis.Config;
using One.Inception.Userfull;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace One.Inception.AtomicAction.Redis.RevisionStore;

internal sealed class RedisRevisionStore : IRevisionStore, IDisposable
{
    private ConnectionMultiplexer connectionDoNotUse;
    private readonly RedisAtomicActionOptions options;
    private readonly ILogger<RedisRevisionStore> logger;

    public RedisRevisionStore(IOptionsMonitor<RedisAtomicActionOptions> options, ILogger<RedisRevisionStore> logger)
    {
        this.options = options.CurrentValue;
        this.logger = logger;
    }

    public Task<Result<bool>> SaveRevisionAsync(string resource, int revision, TimeSpan expiry)
    {
        if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));

        return ExecuteAsync(async (conn) =>
        {
            string revisionKey = CreateRedisRevisionKey(resource);

            bool result = await conn.GetDatabase().StringSetAsync(revisionKey, revision, expiry).ConfigureAwait(false);

            return new Result<bool>(result);
        });
    }

    public Task<Result<int>> PrepareRevisionAsync(string resource, int revision)
    {
        if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException(nameof(resource));

        return ExecuteAsync(async (conn) =>
        {
            string revisionKey = CreateRedisRevisionKey(resource);

            var value = await conn.GetDatabase().StringSetAndGetAsync(revisionKey, revision, options.LongTtl, When.Always, CommandFlags.DemandMaster).ConfigureAwait(false);
            if (value.HasValue == false)
                return new Result<int>(0);

            return new Result<int>((int)value);
        });
    }

    public void Dispose()
    {
        if (connectionDoNotUse != null)
        {
            connectionDoNotUse.Dispose();
            connectionDoNotUse = null;
        }
    }

    private async Task<Result<T>> ExecuteAsync<T>(Func<ConnectionMultiplexer, Task<Result<T>>> theLogic)
    {
        if (connectionDoNotUse is null || (connectionDoNotUse.IsConnected == false && connectionDoNotUse.IsConnecting == false))
        {
            try
            {
                var configurationOptions = ConfigurationOptions.Parse(options.ConnectionString);
                connectionDoNotUse = await ConnectionMultiplexer.ConnectAsync(configurationOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to establish connection with Redis: {connection_string}", options.ConnectionString);
                return Result<T>.FromError(ex);
            }
        }

        try
        {
            return await theLogic(connectionDoNotUse).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to execute Redis query.");

            return Result<T>.FromError(ex);
        }
    }

    private string CreateRedisRevisionKey(string resource) => $"rev:{resource}";

}
