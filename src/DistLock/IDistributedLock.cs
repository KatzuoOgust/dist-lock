namespace KatzuoOgust.DistLock;

/// <summary>
/// Represents a distributed lock for a specific resource.
/// </summary>
public interface IDistributedLock
{
	/// <summary>Gets the resource name this lock targets.</summary>
	public string Resource { get; }

	/// <summary>
	/// Attempts to acquire the lock without waiting.
	/// </summary>
	/// <param name="expiry">How long the lock should be held before it automatically expires.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A handle if the lock was acquired; <c>null</c> otherwise.</returns>
	public Task<IDistributedLockHandle?> TryAcquireAsync(
		TimeSpan expiry,
		CancellationToken cancellationToken = default);
}
