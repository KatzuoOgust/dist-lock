namespace KatzuoOgust.DistLock;

/// <summary>
/// Exception thrown when a distributed lock operation fails.
/// </summary>
public class DistributedLockException : Exception
{
	/// <summary>Gets the resource name the lock was attempted on.</summary>
	public string Resource { get; }

	public DistributedLockException(string resource, string message)
		: base(message)
	{
		Resource = resource;
	}

	public DistributedLockException(string resource, string message, Exception inner)
		: base(message, inner)
	{
		Resource = resource;
	}
}
