using Microsoft.Extensions.Logging;

namespace KatzuoOgust.DistLock.Logging;

/// <summary>
/// Decorator for <see cref="IDistributedLockHandle"/> that adds logging capabilities.
/// </summary>
public class LoggingDistributedLockHandle : IDistributedLockHandle
{
	private readonly IDistributedLockHandle _inner;
	private readonly ILogger<IDistributedLockHandle> _logger;
	private readonly DateTime _acquiredTime;

	/// <summary>
	/// Creates a new instance of <see cref="LoggingDistributedLockHandle"/>.
	/// </summary>
	/// <param name="inner">The underlying lock handle to decorate.</param>
	/// <param name="logger">The logger instance for lock operations.</param>
	public LoggingDistributedLockHandle(IDistributedLockHandle inner, ILogger<IDistributedLockHandle> logger)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_acquiredTime = DateTime.UtcNow;
	}

	/// <summary>
	/// Gets the unique identifier of this lock instance.
	/// </summary>
	public string LockId => _inner.LockId;

	/// <summary>
	/// Gets the resource name this lock was acquired on.
	/// </summary>
	public string Resource => _inner.Resource;

	/// <summary>
	/// Gets whether the lock is currently held.
	/// </summary>
	public bool IsAcquired => _inner.IsAcquired;

	/// <summary>
	/// Releases the lock with logging.
	/// </summary>
	public async Task ReleaseAsync(CancellationToken cancellationToken = default)
	{
		Log.ReleasingLock(_logger, Resource, LockId);

		var duration = DateTime.UtcNow - _acquiredTime;

		try
		{
			await _inner.ReleaseAsync(cancellationToken);
			Log.ReleaseSucceeded(_logger, Resource, LockId, duration);
		}
		catch (OperationCanceledException)
		{
			Log.ReleaseCancelled(_logger, Resource, LockId);
			throw;
		}
		catch (Exception ex)
		{
			Log.ReleaseError(_logger, Resource, LockId, ex);
			throw;
		}
	}

	/// <summary>
	/// Extends the lock expiry with logging.
	/// </summary>
	public async Task<bool> ExtendAsync(TimeSpan expiry, CancellationToken cancellationToken = default)
	{
		Log.ExtendingLock(_logger, Resource, LockId, expiry);

		try
		{
			var result = await _inner.ExtendAsync(expiry, cancellationToken);

			if (result)
			{
				Log.ExtendSucceeded(_logger, Resource, LockId, expiry);
			}
			else
			{
				Log.ExtendFailed(_logger, Resource, LockId);
			}

			return result;
		}
		catch (OperationCanceledException)
		{
			Log.ExtendCancelled(_logger, Resource, LockId);
			throw;
		}
		catch (Exception ex)
		{
			Log.ExtendError(_logger, Resource, LockId, ex);
			throw;
		}
	}

	/// <summary>
	/// Disposes the lock handle.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		try
		{
			await _inner.DisposeAsync();
		}
		catch (Exception ex)
		{
			Log.DisposeError(_logger, Resource, LockId, ex);
			throw;
		}
	}

	private static class Log
	{
		public static void ReleasingLock(ILogger logger, string resource, string lockId)
			=> logger.LogDebug(
				"Releasing lock for resource '{Resource}' (ID: {LockId})",
				resource,
				lockId);

		public static void ReleaseSucceeded(ILogger logger, string resource, string lockId, TimeSpan duration)
			=> logger.LogInformation(
				"Lock released for resource '{Resource}' (ID: {LockId}), held for {DurationMs}ms",
				resource,
				lockId,
				duration.TotalMilliseconds);

		public static void ReleaseCancelled(ILogger logger, string resource, string lockId)
			=> logger.LogWarning(
				"Lock release cancelled for resource '{Resource}' (ID: {LockId})",
				resource,
				lockId);

		public static void ReleaseError(ILogger logger, string resource, string lockId, Exception ex)
			=> logger.LogError(
				ex,
				"Error releasing lock for resource '{Resource}' (ID: {LockId})",
				resource,
				lockId);

		public static void ExtendingLock(ILogger logger, string resource, string lockId, TimeSpan expiry)
			=> logger.LogDebug(
				"Extending lock for resource '{Resource}' (ID: {LockId}) by {ExpiryMs}ms",
				resource,
				lockId,
				expiry.TotalMilliseconds);

		public static void ExtendSucceeded(ILogger logger, string resource, string lockId, TimeSpan expiry)
			=> logger.LogInformation(
				"Lock extended for resource '{Resource}' (ID: {LockId}) by {ExpiryMs}ms",
				resource,
				lockId,
				expiry.TotalMilliseconds);

		public static void ExtendFailed(ILogger logger, string resource, string lockId)
			=> logger.LogWarning(
				"Failed to extend lock for resource '{Resource}' (ID: {LockId}), lock was lost",
				resource,
				lockId);

		public static void ExtendCancelled(ILogger logger, string resource, string lockId)
			=> logger.LogWarning(
				"Lock extension cancelled for resource '{Resource}' (ID: {LockId})",
				resource,
				lockId);

		public static void ExtendError(ILogger logger, string resource, string lockId, Exception ex)
			=> logger.LogError(
				ex,
				"Error extending lock for resource '{Resource}' (ID: {LockId})",
				resource,
				lockId);

		public static void DisposeError(ILogger logger, string resource, string lockId, Exception ex)
			=> logger.LogError(
				ex,
				"Error disposing lock for resource '{Resource}' (ID: {LockId})",
				resource,
				lockId);
	}
}
