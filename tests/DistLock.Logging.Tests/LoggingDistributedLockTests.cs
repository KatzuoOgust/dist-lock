using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace KatzuoOgust.DistLock.Logging;

public class LoggingDistributedLockTests
{
	[Fact]
	public async Task TryAcquireAsync_WhenSuccessful_ReturnsLoggingHandle()
	{
		var mockLock = new Mock<IDistributedLock>();
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLock>>();
		var mockLoggerFactory = new Mock<ILoggerFactory>();
		var mockHandleLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockLock.Setup(l => l.Resource).Returns("test-resource");
		mockHandle.Setup(h => h.LockId).Returns("lock-123");
		mockLock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(mockHandle.Object);
		mockLoggerFactory.Setup(f => f.CreateLogger(typeof(IDistributedLockHandle).FullName!))
			.Returns(mockHandleLogger.Object);

		var loggingLock = new LoggingDistributedLock(mockLock.Object, mockLogger.Object, mockLoggerFactory.Object);
		var result = await loggingLock.TryAcquireAsync(TimeSpan.FromSeconds(10));

		Assert.NotNull(result);
		Assert.IsType<LoggingDistributedLockHandle>(result);
	}

	[Fact]
	public async Task TryAcquireAsync_WhenFailed_ReturnsNull()
	{
		var mockLock = new Mock<IDistributedLock>();
		var mockLogger = new Mock<ILogger<IDistributedLock>>();
		var mockLoggerFactory = new Mock<ILoggerFactory>();

		mockLock.Setup(l => l.Resource).Returns("test-resource");
		mockLock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((IDistributedLockHandle?)null);

		var loggingLock = new LoggingDistributedLock(mockLock.Object, mockLogger.Object, mockLoggerFactory.Object);
		var result = await loggingLock.TryAcquireAsync(TimeSpan.FromSeconds(10));

		Assert.Null(result);
	}

	[Fact]
	public async Task TryAcquireAsync_WhenCancelled_RethrowsOperationCanceledException()
	{
		var mockLock = new Mock<IDistributedLock>();
		var mockLogger = new Mock<ILogger<IDistributedLock>>();
		var mockLoggerFactory = new Mock<ILoggerFactory>();

		mockLock.Setup(l => l.Resource).Returns("test-resource");
		mockLock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new OperationCanceledException());

		var loggingLock = new LoggingDistributedLock(mockLock.Object, mockLogger.Object, mockLoggerFactory.Object);

		await Assert.ThrowsAsync<OperationCanceledException>(() =>
			loggingLock.TryAcquireAsync(TimeSpan.FromSeconds(10)));
	}

	[Fact]
	public async Task TryAcquireAsync_WhenExceptionThrown_RethrowsException()
	{
		var mockLock = new Mock<IDistributedLock>();
		var mockLogger = new Mock<ILogger<IDistributedLock>>();
		var mockLoggerFactory = new Mock<ILoggerFactory>();

		mockLock.Setup(l => l.Resource).Returns("test-resource");
		mockLock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Test error"));

		var loggingLock = new LoggingDistributedLock(mockLock.Object, mockLogger.Object, mockLoggerFactory.Object);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			loggingLock.TryAcquireAsync(TimeSpan.FromSeconds(10)));
	}

	[Fact]
	public void Constructor_WithNullLock_ThrowsArgumentNullException()
	{
		var mockLogger = new Mock<ILogger<IDistributedLock>>();
		var mockLoggerFactory = new Mock<ILoggerFactory>();
		Assert.Throws<ArgumentNullException>(() =>
			new LoggingDistributedLock(null!, mockLogger.Object, mockLoggerFactory.Object));
	}

	[Fact]
	public void Constructor_WithNullLogger_ThrowsArgumentNullException()
	{
		var mockLock = new Mock<IDistributedLock>();
		var mockLoggerFactory = new Mock<ILoggerFactory>();
		Assert.Throws<ArgumentNullException>(() =>
			new LoggingDistributedLock(mockLock.Object, null!, mockLoggerFactory.Object));
	}

	[Fact]
	public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
	{
		var mockLock = new Mock<IDistributedLock>();
		var mockLogger = new Mock<ILogger<IDistributedLock>>();
		Assert.Throws<ArgumentNullException>(() =>
			new LoggingDistributedLock(mockLock.Object, mockLogger.Object, null!));
	}

	[Fact]
	public void Resource_ReturnsInnerResource()
	{
		var mockLock = new Mock<IDistributedLock>();
		var mockLogger = new Mock<ILogger<IDistributedLock>>();
		var mockLoggerFactory = new Mock<ILoggerFactory>();

		mockLock.Setup(l => l.Resource).Returns("my-resource");

		var loggingLock = new LoggingDistributedLock(mockLock.Object, mockLogger.Object, mockLoggerFactory.Object);

		Assert.Equal("my-resource", loggingLock.Resource);
	}
}
