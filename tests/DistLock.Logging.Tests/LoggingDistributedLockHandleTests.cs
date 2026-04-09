using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace KatzuoOgust.DistLock.Logging;

public class LoggingDistributedLockHandleTests
{
	[Fact]
	public async Task ReleaseAsync_WhenSuccessful_LogsRelease()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockHandle.Setup(h => h.Resource).Returns("test-resource");
		mockHandle.Setup(h => h.LockId).Returns("lock-123");
		mockHandle.Setup(h => h.ReleaseAsync(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		var loggingHandle = new LoggingDistributedLockHandle(mockHandle.Object, mockLogger.Object);
		await loggingHandle.ReleaseAsync();

		mockHandle.Verify(h => h.ReleaseAsync(It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task ReleaseAsync_WhenCancelled_RethrowsOperationCanceledException()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockHandle.Setup(h => h.Resource).Returns("test-resource");
		mockHandle.Setup(h => h.LockId).Returns("lock-123");
		mockHandle.Setup(h => h.ReleaseAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new OperationCanceledException());

		var loggingHandle = new LoggingDistributedLockHandle(mockHandle.Object, mockLogger.Object);

		await Assert.ThrowsAsync<OperationCanceledException>(() =>
			loggingHandle.ReleaseAsync());
	}

	[Fact]
	public async Task ReleaseAsync_WhenExceptionThrown_RethrowsException()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockHandle.Setup(h => h.Resource).Returns("test-resource");
		mockHandle.Setup(h => h.LockId).Returns("lock-123");
		mockHandle.Setup(h => h.ReleaseAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Test error"));

		var loggingHandle = new LoggingDistributedLockHandle(mockHandle.Object, mockLogger.Object);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			loggingHandle.ReleaseAsync());
	}

	[Fact]
	public async Task ExtendAsync_WhenSuccessful_ReturnsTrue()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockHandle.Setup(h => h.Resource).Returns("test-resource");
		mockHandle.Setup(h => h.LockId).Returns("lock-123");
		mockHandle.Setup(h => h.ExtendAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		var loggingHandle = new LoggingDistributedLockHandle(mockHandle.Object, mockLogger.Object);
		var result = await loggingHandle.ExtendAsync(TimeSpan.FromSeconds(10));

		Assert.True(result);
	}

	[Fact]
	public async Task ExtendAsync_WhenFailed_ReturnsFalse()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockHandle.Setup(h => h.Resource).Returns("test-resource");
		mockHandle.Setup(h => h.LockId).Returns("lock-123");
		mockHandle.Setup(h => h.ExtendAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		var loggingHandle = new LoggingDistributedLockHandle(mockHandle.Object, mockLogger.Object);
		var result = await loggingHandle.ExtendAsync(TimeSpan.FromSeconds(10));

		Assert.False(result);
	}

	[Fact]
	public async Task ExtendAsync_WhenCancelled_RethrowsOperationCanceledException()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockHandle.Setup(h => h.Resource).Returns("test-resource");
		mockHandle.Setup(h => h.LockId).Returns("lock-123");
		mockHandle.Setup(h => h.ExtendAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new OperationCanceledException());

		var loggingHandle = new LoggingDistributedLockHandle(mockHandle.Object, mockLogger.Object);

		await Assert.ThrowsAsync<OperationCanceledException>(() =>
			loggingHandle.ExtendAsync(TimeSpan.FromSeconds(10)));
	}

	[Fact]
	public async Task DisposeAsync_CallsInnerDisposeAsync()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockHandle.Setup(h => h.Resource).Returns("test-resource");
		mockHandle.Setup(h => h.LockId).Returns("lock-123");
		mockHandle.Setup(h => h.DisposeAsync())
			.Returns(ValueTask.CompletedTask);

		var loggingHandle = new LoggingDistributedLockHandle(mockHandle.Object, mockLogger.Object);
		await loggingHandle.DisposeAsync();

		mockHandle.Verify(h => h.DisposeAsync(), Times.Once);
	}

	[Fact]
	public void Constructor_WithNullHandle_ThrowsArgumentNullException()
	{
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();
		Assert.Throws<ArgumentNullException>(() =>
			new LoggingDistributedLockHandle(null!, mockLogger.Object));
	}

	[Fact]
	public void Constructor_WithNullLogger_ThrowsArgumentNullException()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		Assert.Throws<ArgumentNullException>(() =>
			new LoggingDistributedLockHandle(mockHandle.Object, null!));
	}

	[Fact]
	public void LockId_ReturnsInnerLockId()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockHandle.Setup(h => h.LockId).Returns("lock-999");

		var loggingHandle = new LoggingDistributedLockHandle(mockHandle.Object, mockLogger.Object);

		Assert.Equal("lock-999", loggingHandle.LockId);
	}

	[Fact]
	public void Resource_ReturnsInnerResource()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockHandle.Setup(h => h.Resource).Returns("my-resource");

		var loggingHandle = new LoggingDistributedLockHandle(mockHandle.Object, mockLogger.Object);

		Assert.Equal("my-resource", loggingHandle.Resource);
	}

	[Fact]
	public void IsAcquired_ReturnsInnerIsAcquired()
	{
		var mockHandle = new Mock<IDistributedLockHandle>();
		var mockLogger = new Mock<ILogger<IDistributedLockHandle>>();

		mockHandle.Setup(h => h.IsAcquired).Returns(true);

		var loggingHandle = new LoggingDistributedLockHandle(mockHandle.Object, mockLogger.Object);

		Assert.True(loggingHandle.IsAcquired);
	}
}
