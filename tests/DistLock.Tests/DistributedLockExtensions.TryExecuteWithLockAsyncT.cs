using Moq;

namespace KatzuoOgust.DistLock;

public partial class DistributedLockExtensionsTests
{
	[Fact]
	public async Task TryExecuteWithLockAsync_ReturnsAcquiredFalseAndDefault_WhenLockNotAvailable()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		var (acquired, result) = await _lockMock.Object.TryExecuteWithLockAsync(
			_ => Task.FromResult(99),
			expiry: TimeSpan.FromSeconds(30));

		Assert.False(acquired);
		Assert.Equal(0, result);
	}

	[Fact]
	public async Task TryExecuteWithLockAsync_ReturnsAcquiredTrueAndResult_WhenLockAcquired()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		var (acquired, result) = await _lockMock.Object.TryExecuteWithLockAsync(
			_ => Task.FromResult(42),
			expiry: TimeSpan.FromSeconds(30));

		Assert.True(acquired);
		Assert.Equal(42, result);
	}

	[Fact]
	public async Task TryExecuteWithLockAsync_DisposesHandle_AfterFunc()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		await _lockMock.Object.TryExecuteWithLockAsync(
			_ => Task.FromResult(1),
			expiry: TimeSpan.FromSeconds(30));

		_handleMock.Verify(h => h.DisposeAsync(), Times.Once());
	}
}
