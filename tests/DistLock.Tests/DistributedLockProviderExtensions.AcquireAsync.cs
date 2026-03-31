using Moq;

namespace KatzuoOgust.DistLock;

public partial class DistributedLockProviderExtensionsTests
{
	[Fact]
	public async Task AcquireAsync_ReturnsHandle_WhenLockAcquiredImmediately()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		IDistributedLockHandle handle = await _providerMock.Object.AcquireAsync(
			"test-resource",
			expiry: TimeSpan.FromSeconds(30),
			wait: TimeSpan.FromSeconds(5));

		Assert.Same(_handleMock.Object, handle);
	}

	[Fact]
	public async Task AcquireAsync_ThrowsDistributedLockException_WhenWaitExpires()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		await Assert.ThrowsAsync<DistributedLockException>(() =>
			_providerMock.Object.AcquireAsync(
				"test-resource",
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.Zero));
	}

	[Fact]
	public async Task AcquireAsync_ThrowsDistributedLockException_WithResourceName()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		var ex = await Assert.ThrowsAsync<DistributedLockException>(() =>
			_providerMock.Object.AcquireAsync(
				"test-resource",
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.Zero));

		Assert.Equal("test-resource", ex.Resource);
	}

	[Fact]
	public async Task AcquireAsync_CallsCreateLock_WithResource()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		await _providerMock.Object.AcquireAsync(
			"test-resource",
			expiry: TimeSpan.FromSeconds(30),
			wait: TimeSpan.FromSeconds(5));

		_providerMock.Verify(p => p.CreateLock("test-resource"), Times.Once());
	}

	[Fact]
	public async Task AcquireAsync_ThrowsArgumentNullException_WhenProviderIsNull()
	{
		IDistributedLockProvider? nullProvider = null;
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			nullProvider!.AcquireAsync(
				"test-resource",
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(5)));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public async Task AcquireAsync_ThrowsArgumentException_WhenResourceIsNullOrEmpty(string? resource)
	{
		await Assert.ThrowsAnyAsync<ArgumentException>(() =>
			_providerMock.Object.AcquireAsync(
				resource!,
				expiry: TimeSpan.FromSeconds(30),
				wait: TimeSpan.FromSeconds(5)));
	}
}
