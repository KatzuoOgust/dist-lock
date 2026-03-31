using Moq;

namespace KatzuoOgust.DistLock;

public partial class DistributedLockProviderExtensionsTests
{
	[Fact]
	public async Task ExecuteWithLockAsync_RunsAction_WhenLockAcquired()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		var ran = false;
		await _providerMock.Object.ExecuteWithLockAsync(
			"test-resource",
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

		await _providerMock.Object.ExecuteWithLockAsync(
			"test-resource",
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
			_providerMock.Object.ExecuteWithLockAsync(
				"test-resource",
				_ => Task.CompletedTask,
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.Zero));
	}

	[Fact]
	public async Task ExecuteWithLockAsync_ReturnsResult_WhenLockAcquired()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		var result = await _providerMock.Object.ExecuteWithLockAsync(
			"test-resource",
			_ => Task.FromResult(42),
			expiry: TimeSpan.FromSeconds(30),
			wait: TimeSpan.FromSeconds(5));

		Assert.Equal(42, result);
	}

	[Fact]
	public async Task ExecuteWithLockAsync_ThrowsArgumentNullException_WhenProviderIsNull()
	{
		IDistributedLockProvider? nullProvider = null;
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			nullProvider!.ExecuteWithLockAsync(
				"test-resource",
				_ => Task.CompletedTask,
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(5)));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public async Task ExecuteWithLockAsync_ThrowsArgumentException_WhenResourceIsNullOrEmpty(string? resource)
	{
		await Assert.ThrowsAnyAsync<ArgumentException>(() =>
			_providerMock.Object.ExecuteWithLockAsync(
				resource!,
				_ => Task.CompletedTask,
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(5)));
	}

	[Fact]
	public async Task ExecuteWithLockAsync_ThrowsArgumentNullException_WhenActionIsNull()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			_providerMock.Object.ExecuteWithLockAsync(
				"test-resource",
				action: null!,
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(5)));
	}

	[Fact]
	public async Task ExecuteWithLockAsyncT_ReturnsResult_WhenLockAcquired()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		var result = await _providerMock.Object.ExecuteWithLockAsync(
			"test-resource",
			_ => Task.FromResult(99),
			expiry: TimeSpan.FromSeconds(30),
			wait: TimeSpan.FromSeconds(5));

		Assert.Equal(99, result);
	}

	[Fact]
	public async Task ExecuteWithLockAsyncT_ThrowsArgumentNullException_WhenProviderIsNull()
	{
		IDistributedLockProvider? nullProvider = null;
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			nullProvider!.ExecuteWithLockAsync(
				"test-resource",
				_ => Task.FromResult(42),
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(5)));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public async Task ExecuteWithLockAsyncT_ThrowsArgumentException_WhenResourceIsNullOrEmpty(string? resource)
	{
		await Assert.ThrowsAnyAsync<ArgumentException>(() =>
			_providerMock.Object.ExecuteWithLockAsync(
				resource!,
				_ => Task.FromResult(42),
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(5)));
	}

	[Fact]
	public async Task ExecuteWithLockAsyncT_ThrowsArgumentNullException_WhenFuncIsNull()
	{
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			_providerMock.Object.ExecuteWithLockAsync(
				"test-resource",
				func: (Func<CancellationToken, Task<int>>)null!,
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(5)));
	}
}
