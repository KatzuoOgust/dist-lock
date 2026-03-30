namespace KatzuoOgust.DistLock;

/// <summary>
/// Represents an acquired distributed lock that must be released when no longer needed.
/// </summary>
public interface IDistributedLockHandle : IAsyncDisposable
{
	/// <summary>Gets the unique identifier of this lock instance.</summary>
	public string LockId { get; }

	/// <summary>Gets the resource name this lock was acquired on.</summary>
	public string Resource { get; }

	/// <summary>Gets whether the lock is currently held.</summary>
	public bool IsAcquired { get; }

	/// <summary>Releases the lock explicitly.</summary>
	public Task ReleaseAsync(CancellationToken cancellationToken = default);

	/// <summary>Extends the lock expiry by <paramref name="expiry"/>.</summary>
	/// <returns><c>true</c> if the extension succeeded; <c>false</c> if the lock was already lost.</returns>
	public Task<bool> ExtendAsync(TimeSpan expiry, CancellationToken cancellationToken = default);
}
