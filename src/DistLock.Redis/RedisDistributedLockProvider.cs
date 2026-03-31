using StackExchange.Redis;

namespace KatzuoOgust.DistLock;

/// <summary>
/// Creates <see cref="IDistributedLock"/> instances backed by Redis.
/// </summary>
public sealed class RedisDistributedLockProvider : IDistributedLockProvider
{
	private readonly IDatabase _database;
	private readonly string _keyPrefix;

	/// <param name="database">The Redis database to use for locking.</param>
	/// <param name="keyPrefix">Optional key prefix applied to all resource names.</param>
	public RedisDistributedLockProvider(IDatabase database, string keyPrefix = "distlock:")
	{
		ArgumentNullException.ThrowIfNull(database);
		_database = database;
		_keyPrefix = keyPrefix ?? string.Empty;
	}

	/// <inheritdoc/>
	public IDistributedLock CreateLock(string resource)
	{
		ArgumentException.ThrowIfNullOrEmpty(resource);
		return new RedisDistributedLock(_database, resource, _keyPrefix + resource);
	}
}
