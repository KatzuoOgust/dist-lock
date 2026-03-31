using Moq;

namespace KatzuoOgust.DistLock;

public partial class DistributedLockExtensionsTests
{
	[Fact]
	public async Task AcquireAsync_ReturnsHandle_WhenLockAcquiredImmediately()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		IDistributedLockHandle handle = await _lockMock.Object.AcquireAsync(
			expiry: TimeSpan.FromSeconds(30),
			wait: TimeSpan.FromSeconds(5));

		Assert.Same(_handleMock.Object, handle);
	}

	[Fact]
	public async Task AcquireAsync_ReturnsHandle_WhenLockAcquiredAfterRetry()
	{
		var callCount = 0;
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(() => ++callCount < 4 ? null : _handleMock.Object);

		IDistributedLockHandle handle = await _lockMock.Object.AcquireAsync(
			expiry: TimeSpan.FromSeconds(30),
			wait: TimeSpan.FromSeconds(10));

		Assert.Same(_handleMock.Object, handle);
		Assert.Equal(4, callCount);
	}

	[Fact]
	public async Task AcquireAsync_ThrowsDistributedLockException_WhenWaitExpires()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		await Assert.ThrowsAsync<DistributedLockException>(() =>
			_lockMock.Object.AcquireAsync(
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.Zero));
	}

	[Fact]
	public async Task AcquireAsync_ThrowsDistributedLockException_WithResourceName()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		var ex = await Assert.ThrowsAsync<DistributedLockException>(() =>
			_lockMock.Object.AcquireAsync(
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.Zero));

		Assert.Equal("test-resource", ex.Resource);
	}

	[Fact]
	public async Task AcquireAsync_PropagatesCancellation_WhenTokenCancelled()
	{
		using var cts = new CancellationTokenSource();
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			_lockMock.Object.AcquireAsync(
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(10),
				cancellationToken: cts.Token));
	}

	[Fact]
	public async Task AcquireAsync_ThrowsOperationCanceledException_WhenTokenCancelledAndWaitIsZero()
	{
		// Regression: with wait=0 the deadline check used to fire before the cancellation
		// check, producing DistributedLockException instead of OperationCanceledException.
		using var cts = new CancellationTokenSource();
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		cts.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			_lockMock.Object.AcquireAsync(
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.Zero,
				cancellationToken: cts.Token));
	}

	[Fact]
	public async Task AcquireAsync_ThrowsArgumentNullException_WhenLockIsNull()
	{
		IDistributedLock? nullLock = null;
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			nullLock!.AcquireAsync(
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(5)));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task AcquireAsync_ThrowsArgumentOutOfRangeException_WhenExpiryIsNotPositive(int expirySeconds)
	{
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			_lockMock.Object.AcquireAsync(
				expiry: TimeSpan.FromSeconds(expirySeconds),
				wait: TimeSpan.FromSeconds(5)));
	}

	[Fact]
	public async Task AcquireAsync_ThrowsArgumentOutOfRangeException_WhenWaitIsNegative()
	{
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			_lockMock.Object.AcquireAsync(
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(-1)));
	}
}
