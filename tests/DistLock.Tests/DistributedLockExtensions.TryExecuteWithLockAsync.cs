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

	[Fact]
	public async Task TryExecuteWithLockAsync_ThrowsArgumentNullException_WhenLockIsNull()
	{
		IDistributedLock? nullLock = null;
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			nullLock!.TryExecuteWithLockAsync(
				_ => Task.CompletedTask,
				expiry: TimeSpan.FromSeconds(30)));
	}

	[Fact]
	public async Task TryExecuteWithLockAsync_ThrowsArgumentNullException_WhenActionIsNull()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			_lockMock.Object.TryExecuteWithLockAsync(
				action: null!,
				expiry: TimeSpan.FromSeconds(30)));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task TryExecuteWithLockAsync_ThrowsArgumentOutOfRangeException_WhenExpiryIsNotPositive(int expirySeconds)
	{
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			_lockMock.Object.TryExecuteWithLockAsync(
				_ => Task.CompletedTask,
				expiry: TimeSpan.FromSeconds(expirySeconds)));
	}
}
