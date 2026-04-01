namespace KatzuoOgust.DistLock;

/// <summary>
/// Extension methods for <see cref="IDistributedLock"/>.
/// </summary>
public static class DistributedLockExtensions
{
	private static readonly TimeSpan _retryInitial = TimeSpan.FromMilliseconds(50);
	private static readonly TimeSpan _retryMax = TimeSpan.FromSeconds(1);

	extension(IDistributedLock @lock)
	{
		/// <summary>
		/// Acquires the lock, waiting up to <paramref name="wait"/> for it to become available.
		/// Polls <see cref="IDistributedLock.TryAcquireAsync"/> with exponential back-off (50 ms → 100 ms → …,
		/// capped at 1 s) until the lock is acquired or the wait period expires.
		/// </summary>
		/// <param name="expiry">How long the lock should be held before it automatically expires.</param>
		/// <param name="wait">How long to wait for the lock.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>A handle representing the acquired lock.</returns>
		/// <exception cref="DistributedLockException">Thrown when the lock cannot be acquired within <paramref name="wait"/>.</exception>
		public async Task<IDistributedLockHandle> AcquireAsync(
			TimeSpan expiry,
			TimeSpan wait,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(@lock);
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiry, TimeSpan.Zero);
			ArgumentOutOfRangeException.ThrowIfLessThan(wait, TimeSpan.Zero);

			DateTime deadline = DateTime.UtcNow + wait;
			TimeSpan delay = _retryInitial;

			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();

				IDistributedLockHandle? handle = await @lock.TryAcquireAsync(expiry, cancellationToken).ConfigureAwait(false);
				if (handle is not null)
					return handle;

				TimeSpan remaining = deadline - DateTime.UtcNow;
				if (remaining <= TimeSpan.Zero)
					throw Error.LockNotAcquired(@lock.Resource, wait);

				TimeSpan sleep = delay < remaining ? delay : remaining;
				await Task.Delay(sleep, cancellationToken).ConfigureAwait(false);

				delay = delay.Ticks * 2 < _retryMax.Ticks ? TimeSpan.FromTicks(delay.Ticks * 2) : _retryMax;
			}
		}

		/// <summary>
		/// Acquires the lock and executes <paramref name="action"/>, then releases the lock.
		/// Waits up to <paramref name="wait"/> for the lock to become available.
		/// </summary>
		public async Task ExecuteWithLockAsync(
			Func<CancellationToken, Task> action,
			TimeSpan expiry,
			TimeSpan wait,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(@lock);
			ArgumentNullException.ThrowIfNull(action);
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiry, TimeSpan.Zero);
			ArgumentOutOfRangeException.ThrowIfLessThan(wait, TimeSpan.Zero);

			await using IDistributedLockHandle handle = await @lock.AcquireAsync(expiry, wait, cancellationToken).ConfigureAwait(false);
			await action(cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Acquires the lock and executes <paramref name="func"/>, returning its result, then releases the lock.
		/// Waits up to <paramref name="wait"/> for the lock to become available.
		/// </summary>
		public async Task<T> ExecuteWithLockAsync<T>(
			Func<CancellationToken, Task<T>> func,
			TimeSpan expiry,
			TimeSpan wait,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(@lock);
			ArgumentNullException.ThrowIfNull(func);
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiry, TimeSpan.Zero);
			ArgumentOutOfRangeException.ThrowIfLessThan(wait, TimeSpan.Zero);

			await using IDistributedLockHandle handle = await @lock.AcquireAsync(expiry, wait, cancellationToken).ConfigureAwait(false);
			return await func(cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Attempts to acquire the lock and execute <paramref name="action"/>.
		/// Returns <c>true</c> if the lock was acquired and the action ran; <c>false</c> if the lock
		/// was not available. Any exception thrown by <paramref name="action"/> propagates to the caller.
		/// </summary>
		public async Task<bool> TryExecuteWithLockAsync(
			Func<CancellationToken, Task> action,
			TimeSpan expiry,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(@lock);
			ArgumentNullException.ThrowIfNull(action);
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiry, TimeSpan.Zero);

			await using IDistributedLockHandle? handle = await @lock.TryAcquireAsync(expiry, cancellationToken).ConfigureAwait(false);
			if (handle is null)
				return false;

			await action(cancellationToken).ConfigureAwait(false);
			return true;
		}

		/// <summary>
		/// Attempts to acquire the lock and execute <paramref name="func"/>, returning its result.
		/// Returns <c>(true, result)</c> if the lock was acquired; <c>(false, default)</c> if the lock
		/// was not available. Any exception thrown by <paramref name="func"/> propagates to the caller.
		/// </summary>
		public async Task<(bool Acquired, T? Result)> TryExecuteWithLockAsync<T>(
			Func<CancellationToken, Task<T>> func,
			TimeSpan expiry,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(@lock);
			ArgumentNullException.ThrowIfNull(func);
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiry, TimeSpan.Zero);

			await using IDistributedLockHandle? handle = await @lock.TryAcquireAsync(expiry, cancellationToken).ConfigureAwait(false);
			if (handle is null)
				return (false, default);

			T result = await func(cancellationToken).ConfigureAwait(false);
			return (true, result);
		}
	}

	private static class Error
	{
		public static DistributedLockException LockNotAcquired(string resource, TimeSpan wait) =>
			new(resource, $"Could not acquire lock on '{resource}' within {wait}.");
	}
}
