using Microsoft.Extensions.Logging;

namespace KatzuoOgust.DistLock.Logging;

/// <summary>
/// Decorator for <see cref="IDistributedLockProvider"/> that adds logging capabilities.
/// </summary>
public class LoggingDistributedLockProvider : IDistributedLockProvider
{
	private readonly IDistributedLockProvider _inner;
	private readonly ILogger<IDistributedLockProvider> _logger;
	private readonly ILoggerFactory _loggerFactory;

	/// <summary>
	/// Creates a new instance of <see cref="LoggingDistributedLockProvider"/>.
	/// </summary>
	/// <param name="inner">The underlying lock provider to decorate.</param>
	/// <param name="logger">The logger instance for lock operations.</param>
	/// <param name="loggerFactory">The logger factory for creating lock loggers.</param>
	public LoggingDistributedLockProvider(IDistributedLockProvider inner, ILogger<IDistributedLockProvider> logger, ILoggerFactory loggerFactory)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
	}

	/// <summary>
	/// Creates a lock with logging capabilities.
	/// </summary>
	public IDistributedLock CreateLock(string resource)
	{
		_logger.LogDebug("Creating lock for resource '{Resource}'", resource);
		var innerLock = _inner.CreateLock(resource);
		var lockLogger = _loggerFactory.CreateLogger<IDistributedLock>();
		return new LoggingDistributedLock(innerLock, lockLogger, _loggerFactory);
	}
}
