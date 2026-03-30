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

	[Fact]
	public async Task TryExecuteWithLockAsyncT_ThrowsArgumentNullException_WhenLockIsNull()
	{
		IDistributedLock? nullLock = null;
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			nullLock!.TryExecuteWithLockAsync(
				_ => Task.FromResult(42),
				expiry: TimeSpan.FromSeconds(30)));
	}

	[Fact]
	public async Task TryExecuteWithLockAsyncT_ThrowsArgumentNullException_WhenFuncIsNull()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			_lockMock.Object.TryExecuteWithLockAsync(
				func: (Func<CancellationToken, Task<int>>)null!,
				expiry: TimeSpan.FromSeconds(30)));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task TryExecuteWithLockAsyncT_ThrowsArgumentOutOfRangeException_WhenExpiryIsNotPositive(int expirySeconds)
	{
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			_lockMock.Object.TryExecuteWithLockAsync(
				_ => Task.FromResult(42),
				expiry: TimeSpan.FromSeconds(expirySeconds)));
	}
}
