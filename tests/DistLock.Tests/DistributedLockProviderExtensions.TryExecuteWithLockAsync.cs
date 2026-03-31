using Moq;

namespace KatzuoOgust.DistLock;

public partial class DistributedLockProviderExtensionsTests
{
	[Fact]
	public async Task TryExecuteWithLockAsync_ReturnsFalse_WhenLockNotAvailable()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		var ran = false;
		var result = await _providerMock.Object.TryExecuteWithLockAsync(
			"test-resource",
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

		var result = await _providerMock.Object.TryExecuteWithLockAsync(
			"test-resource",
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
		await _providerMock.Object.TryExecuteWithLockAsync(
			"test-resource",
			_ => { ran = true; return Task.CompletedTask; },
			expiry: TimeSpan.FromSeconds(30));

		Assert.True(ran);
	}

	[Fact]
	public async Task TryExecuteWithLockAsync_DisposesHandle_AfterAction()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		await _providerMock.Object.TryExecuteWithLockAsync(
			"test-resource",
			_ => Task.CompletedTask,
			expiry: TimeSpan.FromSeconds(30));

		_handleMock.Verify(h => h.DisposeAsync(), Times.Once());
	}

	[Fact]
	public async Task TryExecuteWithLockAsync_ThrowsArgumentNullException_WhenProviderIsNull()
	{
		IDistributedLockProvider? nullProvider = null;
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			nullProvider!.TryExecuteWithLockAsync(
				"test-resource",
				_ => Task.CompletedTask,
				expiry: TimeSpan.FromSeconds(30)));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public async Task TryExecuteWithLockAsync_ThrowsArgumentException_WhenResourceIsNullOrEmpty(string? resource)
	{
		await Assert.ThrowsAnyAsync<ArgumentException>(() =>
			_providerMock.Object.TryExecuteWithLockAsync(
				resource!,
				_ => Task.CompletedTask,
				expiry: TimeSpan.FromSeconds(30)));
	}

	[Fact]
	public async Task TryExecuteWithLockAsync_ThrowsArgumentNullException_WhenActionIsNull()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			_providerMock.Object.TryExecuteWithLockAsync(
				"test-resource",
				action: null!,
				expiry: TimeSpan.FromSeconds(30)));
	}

	[Fact]
	public async Task TryExecuteWithLockAsyncT_ReturnsAcquiredFalseAndDefault_WhenLockNotAvailable()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		var (acquired, result) = await _providerMock.Object.TryExecuteWithLockAsync(
			"test-resource",
			_ => Task.FromResult(99),
			expiry: TimeSpan.FromSeconds(30));

		Assert.False(acquired);
		Assert.Equal(0, result);
	}

	[Fact]
	public async Task TryExecuteWithLockAsyncT_ReturnsAcquiredTrueAndResult_WhenLockAcquired()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		var (acquired, result) = await _providerMock.Object.TryExecuteWithLockAsync(
			"test-resource",
			_ => Task.FromResult(42),
			expiry: TimeSpan.FromSeconds(30));

		Assert.True(acquired);
		Assert.Equal(42, result);
	}

	[Fact]
	public async Task TryExecuteWithLockAsyncT_ThrowsArgumentNullException_WhenProviderIsNull()
	{
		IDistributedLockProvider? nullProvider = null;
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			nullProvider!.TryExecuteWithLockAsync(
				"test-resource",
				_ => Task.FromResult(42),
				expiry: TimeSpan.FromSeconds(30)));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public async Task TryExecuteWithLockAsyncT_ThrowsArgumentException_WhenResourceIsNullOrEmpty(string? resource)
	{
		await Assert.ThrowsAnyAsync<ArgumentException>(() =>
			_providerMock.Object.TryExecuteWithLockAsync(
				resource!,
				_ => Task.FromResult(42),
				expiry: TimeSpan.FromSeconds(30)));
	}

	[Fact]
	public async Task TryExecuteWithLockAsyncT_ThrowsArgumentNullException_WhenFuncIsNull()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			_providerMock.Object.TryExecuteWithLockAsync(
				"test-resource",
				func: (Func<CancellationToken, Task<int>>)null!,
				expiry: TimeSpan.FromSeconds(30)));
	}
}
