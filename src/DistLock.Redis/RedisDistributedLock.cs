using StackExchange.Redis;

namespace KatzuoOgust.DistLock;

/// <summary>
/// A distributed lock for a specific resource backed by Redis.
/// </summary>
internal sealed class RedisDistributedLock : IDistributedLock
{
	private readonly IDatabase _database;
	private readonly string _key;

	internal RedisDistributedLock(IDatabase database, string resource, string key)
	{
		_database = database;
		Resource = resource;
		_key = key;
	}

	/// <inheritdoc/>
	public string Resource { get; }

	/// <inheritdoc/>
	public async Task<IDistributedLockHandle?> TryAcquireAsync(
		TimeSpan expiry,
		CancellationToken cancellationToken = default)
	{
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiry, TimeSpan.Zero);

		var lockId = Guid.NewGuid().ToString("N");
		bool acquired = await _database
			.StringSetAsync(_key, lockId, expiry, When.NotExists, CommandFlags.None)
			.ConfigureAwait(false);

		return acquired
			? new RedisDistributedLockHandle(_database, Resource, _key, lockId)
			: null;
	}
}
