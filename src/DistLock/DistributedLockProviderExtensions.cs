namespace KatzuoOgust.DistLock;

/// <summary>
/// Extension methods for <see cref="IDistributedLockProvider"/>.
/// </summary>
public static class DistributedLockProviderExtensions
{
	extension(IDistributedLockProvider provider)
	{
		/// <summary>
		/// Creates a lock for <paramref name="resource"/> and attempts to acquire it without waiting.
		/// </summary>
		/// <param name="resource">The resource name to lock.</param>
		/// <param name="expiry">How long the lock should be held before it automatically expires.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>A handle if the lock was acquired; <c>null</c> otherwise.</returns>
		public async Task<IDistributedLockHandle?> TryAcquireAsync(
			string resource,
			TimeSpan expiry,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(provider);
			ArgumentException.ThrowIfNullOrEmpty(resource);

			return await provider.CreateLock(resource).TryAcquireAsync(expiry, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a lock for <paramref name="resource"/> and acquires it, waiting up to <paramref name="wait"/>.
		/// Polls with exponential back-off until the lock is acquired or the wait period expires.
		/// </summary>
		/// <param name="resource">The resource name to lock.</param>
		/// <param name="expiry">How long the lock should be held before it automatically expires.</param>
		/// <param name="wait">How long to wait for the lock.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>A handle representing the acquired lock.</returns>
		/// <exception cref="DistributedLockException">Thrown when the lock cannot be acquired within <paramref name="wait"/>.</exception>
		public async Task<IDistributedLockHandle> AcquireAsync(
			string resource,
			TimeSpan expiry,
			TimeSpan wait,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(provider);
			ArgumentException.ThrowIfNullOrEmpty(resource);

			return await provider.CreateLock(resource).AcquireAsync(expiry, wait, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a lock for <paramref name="resource"/>, acquires it, executes <paramref name="action"/>,
		/// then releases the lock. Waits up to <paramref name="wait"/> for the lock to become available.
		/// </summary>
		public async Task ExecuteWithLockAsync(
			string resource,
			Func<CancellationToken, Task> action,
			TimeSpan expiry,
			TimeSpan wait,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(provider);
			ArgumentException.ThrowIfNullOrEmpty(resource);
			ArgumentNullException.ThrowIfNull(action);

			await provider.CreateLock(resource).ExecuteWithLockAsync(action, expiry, wait, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a lock for <paramref name="resource"/>, acquires it, executes <paramref name="func"/>,
		/// returns its result, then releases the lock. Waits up to <paramref name="wait"/> for the lock to become available.
		/// </summary>
		public async Task<T> ExecuteWithLockAsync<T>(
			string resource,
			Func<CancellationToken, Task<T>> func,
			TimeSpan expiry,
			TimeSpan wait,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(provider);
			ArgumentException.ThrowIfNullOrEmpty(resource);
			ArgumentNullException.ThrowIfNull(func);

			return await provider.CreateLock(resource).ExecuteWithLockAsync(func, expiry, wait, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a lock for <paramref name="resource"/> and attempts to acquire it and execute
		/// <paramref name="action"/>. Returns <c>true</c> if the lock was acquired and the action ran;
		/// <c>false</c> if the lock was not available.
		/// </summary>
		public async Task<bool> TryExecuteWithLockAsync(
			string resource,
			Func<CancellationToken, Task> action,
			TimeSpan expiry,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(provider);
			ArgumentException.ThrowIfNullOrEmpty(resource);
			ArgumentNullException.ThrowIfNull(action);

			return await provider.CreateLock(resource).TryExecuteWithLockAsync(action, expiry, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a lock for <paramref name="resource"/> and attempts to acquire it and execute
		/// <paramref name="func"/>, returning its result.
		/// Returns <c>(true, result)</c> if acquired; <c>(false, default)</c> otherwise.
		/// </summary>
		public async Task<(bool Acquired, T? Result)> TryExecuteWithLockAsync<T>(
			string resource,
			Func<CancellationToken, Task<T>> func,
			TimeSpan expiry,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(provider);
			ArgumentException.ThrowIfNullOrEmpty(resource);
			ArgumentNullException.ThrowIfNull(func);

			return await provider.CreateLock(resource).TryExecuteWithLockAsync(func, expiry, cancellationToken).ConfigureAwait(false);
		}
	}
}
