using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace KatzuoOgust.DistLock.Logging;

public class LoggingDistributedLockProviderTests
{
	[Fact]
	public void CreateLock_WithValidResource_ReturnsLoggingLock()
	{
		var mockProvider = new Mock<IDistributedLockProvider>();
		var mockLock = new Mock<IDistributedLock>();
		var mockLogger = new Mock<ILogger<IDistributedLockProvider>>();
		var mockLoggerFactory = new Mock<ILoggerFactory>();
		var mockLockLogger = new Mock<ILogger<IDistributedLock>>();

		mockProvider.Setup(p => p.CreateLock("resource1")).Returns(mockLock.Object);
		mockLoggerFactory.Setup(f => f.CreateLogger(typeof(IDistributedLock).FullName!))
			.Returns(mockLockLogger.Object);

		var loggingProvider = new LoggingDistributedLockProvider(mockProvider.Object, mockLogger.Object, mockLoggerFactory.Object);
		var result = loggingProvider.CreateLock("resource1");

		Assert.NotNull(result);
		Assert.IsType<LoggingDistributedLock>(result);
		mockProvider.Verify(p => p.CreateLock("resource1"), Times.Once);
	}

	[Fact]
	public void Constructor_WithNullProvider_ThrowsArgumentNullException()
	{
		var mockLogger = new Mock<ILogger<IDistributedLockProvider>>();
		var mockLoggerFactory = new Mock<ILoggerFactory>();
		Assert.Throws<ArgumentNullException>(() =>
			new LoggingDistributedLockProvider(null!, mockLogger.Object, mockLoggerFactory.Object));
	}

	[Fact]
	public void Constructor_WithNullLogger_ThrowsArgumentNullException()
	{
		var mockProvider = new Mock<IDistributedLockProvider>();
		var mockLoggerFactory = new Mock<ILoggerFactory>();
		Assert.Throws<ArgumentNullException>(() =>
			new LoggingDistributedLockProvider(mockProvider.Object, null!, mockLoggerFactory.Object));
	}

	[Fact]
	public void Constructor_WithNullLoggerFactory_ThrowsArgumentNullException()
	{
		var mockProvider = new Mock<IDistributedLockProvider>();
		var mockLogger = new Mock<ILogger<IDistributedLockProvider>>();
		Assert.Throws<ArgumentNullException>(() =>
			new LoggingDistributedLockProvider(mockProvider.Object, mockLogger.Object, null!));
	}
}
