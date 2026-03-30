namespace KatzuoOgust.DistLock;

/// <summary>
/// Creates <see cref="IDistributedLock"/> instances for named resources.
/// </summary>
public interface IDistributedLockProvider
{
	/// <summary>
	/// Returns a lock for the given <paramref name="resource"/>.
	/// </summary>
	public IDistributedLock CreateLock(string resource);
}
