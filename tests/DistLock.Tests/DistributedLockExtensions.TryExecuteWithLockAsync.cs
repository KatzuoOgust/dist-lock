using Moq;

namespace KatzuoOgust.DistLock;

public partial class DistributedLockExtensionsTests
{
	[Fact]
	public async Task TryExecuteWithLockAsync_ReturnsFalse_WhenLockNotAvailable()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		var ran = false;
		var result = await _lockMock.Object.TryExecuteWithLockAsync(
			_ => { ran = true; return Task.CompletedTask; },
			expiry: TimeSpan.FromSeconds(30));

		Assert.False(result);
		Assert.False(ran);
	}

	[Fact]
	public async Task TryExecuteWithLockAsync_ReturnsTrue_WhenLockAcquired()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		var result = await _lockMock.Object.TryExecuteWithLockAsync(
			_ => Task.CompletedTask,
			expiry: TimeSpan.FromSeconds(30));

		Assert.True(result);
	}

	[Fact]
	public async Task TryExecuteWithLockAsync_RunsAction_WhenLockAcquired()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		var ran = false;
		await _lockMock.Object.TryExecuteWithLockAsync(
			_ => { ran = true; return Task.CompletedTask; },
			expiry: TimeSpan.FromSeconds(30));

		Assert.True(ran);
	}

	[Fact]
	public async Task TryExecuteWithLockAsync_DisposesHandle_AfterAction()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		await _lockMock.Object.TryExecuteWithLockAsync(
			_ => Task.CompletedTask,
			expiry: TimeSpan.FromSeconds(30));

		_handleMock.Verify(h => h.DisposeAsync(), Times.Once());
	}
}
