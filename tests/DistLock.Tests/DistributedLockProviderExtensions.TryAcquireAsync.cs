using Moq;

namespace KatzuoOgust.DistLock;

public partial class DistributedLockProviderExtensionsTests
{
	[Fact]
	public async Task TryAcquireAsync_ReturnsHandle_WhenLockAvailable()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync(_handleMock.Object);

		IDistributedLockHandle? handle = await _providerMock.Object.TryAcquireAsync(
			"test-resource",
			expiry: TimeSpan.FromSeconds(30));

		Assert.Same(_handleMock.Object, handle);
	}

	[Fact]
	public async Task TryAcquireAsync_ReturnsNull_WhenLockNotAvailable()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		IDistributedLockHandle? handle = await _providerMock.Object.TryAcquireAsync(
			"test-resource",
			expiry: TimeSpan.FromSeconds(30));

		Assert.Null(handle);
	}

	[Fact]
	public async Task TryAcquireAsync_CallsCreateLock_WithResource()
	{
		_lockMock.Setup(l => l.TryAcquireAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				 .ReturnsAsync((IDistributedLockHandle?)null);

		await _providerMock.Object.TryAcquireAsync("test-resource", expiry: TimeSpan.FromSeconds(30));

		_providerMock.Verify(p => p.CreateLock("test-resource"), Times.Once());
	}

	[Fact]
	public async Task TryAcquireAsync_ThrowsArgumentNullException_WhenProviderIsNull()
	{
		IDistributedLockProvider? nullProvider = null;
		await Assert.ThrowsAsync<ArgumentNullException>(() =>
			nullProvider!.TryAcquireAsync("test-resource", expiry: TimeSpan.FromSeconds(30)));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public async Task TryAcquireAsync_ThrowsArgumentException_WhenResourceIsNullOrEmpty(string? resource)
	{
		await Assert.ThrowsAnyAsync<ArgumentException>(() =>
			_providerMock.Object.TryAcquireAsync(resource!, expiry: TimeSpan.FromSeconds(30)));
	}
}
