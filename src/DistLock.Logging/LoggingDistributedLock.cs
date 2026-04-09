using Microsoft.Extensions.Logging;

namespace KatzuoOgust.DistLock.Logging;

/// <summary>
/// Decorator for <see cref="IDistributedLock"/> that adds logging capabilities.
/// </summary>
public class LoggingDistributedLock : IDistributedLock
{
	private readonly IDistributedLock _inner;
	private readonly ILogger<IDistributedLock> _logger;
	private readonly ILoggerFactory _loggerFactory;

	/// <summary>
	/// Creates a new instance of <see cref="LoggingDistributedLock"/>.
	/// </summary>
	/// <param name="inner">The underlying lock to decorate.</param>
	/// <param name="logger">The logger instance for lock operations.</param>
	/// <param name="loggerFactory">The logger factory for creating handle loggers.</param>
	public LoggingDistributedLock(IDistributedLock inner, ILogger<IDistributedLock> logger, ILoggerFactory loggerFactory)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
	}

	/// <summary>
	/// Gets the resource name this lock targets.
	/// </summary>
	public string Resource => _inner.Resource;

	/// <summary>
	/// Attempts to acquire the lock with logging.
	/// </summary>
	public async Task<IDistributedLockHandle?> TryAcquireAsync(
		TimeSpan expiry,
		CancellationToken cancellationToken = default)
	{
		Log.AttemptingAcquire(_logger, Resource, expiry);

		var startTime = DateTime.UtcNow;

		try
		{
			var handle = await _inner.TryAcquireAsync(expiry, cancellationToken);

			if (handle != null)
			{
				var duration = DateTime.UtcNow - startTime;
				Log.AcquireSucceeded(_logger, Resource, handle.LockId, duration);

				return new LoggingDistributedLockHandle(handle, _loggerFactory.CreateLogger<IDistributedLockHandle>());
			}

			Log.AcquireFailed(_logger, Resource);
			return null;
		}
		catch (OperationCanceledException)
		{
			Log.AcquireCancelled(_logger, Resource);
			throw;
		}
		catch (Exception ex)
		{
			Log.AcquireError(_logger, Resource, ex);
			throw;
		}
	}

	private static class Log
	{
		public static void AttemptingAcquire(ILogger logger, string resource, TimeSpan expiry)
			=> logger.LogDebug(
				"Attempting to acquire lock for resource '{Resource}' with expiry {ExpiryMs}ms",
				resource,
				expiry.TotalMilliseconds);

		public static void AcquireSucceeded(ILogger logger, string resource, string lockId, TimeSpan duration)
			=> logger.LogInformation(
				"Lock acquired for resource '{Resource}' (ID: {LockId}) in {DurationMs}ms",
				resource,
				lockId,
				duration.TotalMilliseconds);

		public static void AcquireFailed(ILogger logger, string resource)
			=> logger.LogWarning("Failed to acquire lock for resource '{Resource}'", resource);

		public static void AcquireCancelled(ILogger logger, string resource)
			=> logger.LogWarning("Lock acquisition cancelled for resource '{Resource}'", resource);

		public static void AcquireError(ILogger logger, string resource, Exception ex)
			=> logger.LogError(ex, "Error acquiring lock for resource '{Resource}'", resource);
	}
}
