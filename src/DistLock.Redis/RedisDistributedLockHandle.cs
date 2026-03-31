using StackExchange.Redis;

namespace KatzuoOgust.DistLock.Redis;

/// <summary>
/// Represents an acquired Redis-backed distributed lock.
/// Uses Lua scripts to ensure atomic check-and-release / check-and-extend operations.
/// </summary>
internal sealed class RedisDistributedLockHandle : IDistributedLockHandle
{
	// Atomically deletes the key only if its value matches the lock ID.
	private const string ReleaseScript =
		"if redis.call('get', KEYS[1]) == ARGV[1] then " +
		"  return redis.call('del', KEYS[1]) " +
		"else " +
		"  return 0 " +
		"end";

	// Atomically extends the key TTL only if its value matches the lock ID.
	private const string ExtendScript =
		"if redis.call('get', KEYS[1]) == ARGV[1] then " +
		"  return redis.call('pexpire', KEYS[1], ARGV[2]) " +
		"else " +
		"  return 0 " +
		"end";

	private readonly IDatabase _database;
	private readonly string _key;
	private int _released; // 0 = held, 1 = released; mutated via Interlocked

	internal RedisDistributedLockHandle(IDatabase database, string resource, string key, string lockId)
	{
		_database = database;
		Resource = resource;
		_key = key;
		LockId = lockId;
	}

	/// <inheritdoc/>
	public string LockId { get; }

	/// <inheritdoc/>
	public string Resource { get; }

	/// <inheritdoc/>
	public bool IsAcquired => _released == 0;

	/// <inheritdoc/>
	public async Task ReleaseAsync(CancellationToken cancellationToken = default)
	{
		if (Interlocked.CompareExchange(ref _released, 1, 0) != 0)
			return;

		await _database.ScriptEvaluateAsync(
			ReleaseScript,
			keys: [_key],
			values: [LockId]).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<bool> ExtendAsync(TimeSpan expiry, CancellationToken cancellationToken = default)
	{
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiry, TimeSpan.Zero);

		if (Volatile.Read(ref _released) != 0)
			return false;

		RedisResult result = await _database.ScriptEvaluateAsync(
			ExtendScript,
			keys: [_key],
			values: [LockId, (long)expiry.TotalMilliseconds]).ConfigureAwait(false);

		return (long)result == 1;
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		await ReleaseAsync().ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}
}
