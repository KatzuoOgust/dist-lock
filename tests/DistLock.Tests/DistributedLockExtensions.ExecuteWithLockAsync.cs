using Moq;

namespace KatzuoOgust.DistLock;

public partial class DistributedLockExtensionsTests
{
	private readonly Mock<IDistributedLock> _lockMock = new();
	private readonly Mock<IDistributedLockHandle> _handleMock = new();

	public DistributedLockExtensionsTests()
	{
		_lockMock.Setup(l => l.Resource).Returns("test-resource");
		_handleMock.Setup(h => h.IsAcquired).Returns(true);
		_handleMock.Setup(h => h.Resource).Returns("test-resource");
		_handleMock.Setup(h => h.DisposeAsync()).Returns(ValueTask.CompletedTask);
	}

	[Fact]
	public async Task ExecuteWithLockAsync_RunsAction_WhenLockAcquired()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		var ran = false;
		await _lockMock.Object.ExecuteWithLockAsync(
			_ => { ran = true; return Task.CompletedTask; },
			expiry: TimeSpan.FromSeconds(30),
			wait: TimeSpan.FromSeconds(5));

		Assert.True(ran);
	}

	[Fact]
	public async Task ExecuteWithLockAsync_DisposesHandle_AfterAction()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		await _lockMock.Object.ExecuteWithLockAsync(
			_ => Task.CompletedTask,
			expiry: TimeSpan.FromSeconds(30),
			wait: TimeSpan.FromSeconds(5));

		_handleMock.Verify(h => h.DisposeAsync(), Times.Once());
	}

	[Fact]
	public async Task ExecuteWithLockAsync_ThrowsDistributedLockException_WhenLockNotAcquired()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		await Assert.ThrowsAsync<DistributedLockException>(() =>
			_lockMock.Object.ExecuteWithLockAsync(
				_ => Task.CompletedTask,
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.Zero));
	}

	[Fact]
	public async Task ExecuteWithLockAsync_ReturnsResult_WhenLockAcquired()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		var result = await _lockMock.Object.ExecuteWithLockAsync(
			_ => Task.FromResult(42),
			expiry: TimeSpan.FromSeconds(30),
			wait: TimeSpan.FromSeconds(5));

		Assert.Equal(42, result);
	}
}
