using Moq;

namespace KatzuoOgust.DistLock;

public partial class DistributedLockProviderExtensionsTests
{
	private readonly Mock<IDistributedLockProvider> _providerMock = new();
	private readonly Mock<IDistributedLock> _lockMock = new();
	private readonly Mock<IDistributedLockHandle> _handleMock = new();

	public DistributedLockProviderExtensionsTests()
	{
		_lockMock.Setup(l => l.Resource).Returns("test-resource");
		_handleMock.Setup(h => h.IsAcquired).Returns(true);
		_handleMock.Setup(h => h.Resource).Returns("test-resource");
		_handleMock.Setup(h => h.DisposeAsync()).Returns(ValueTask.CompletedTask);
		_providerMock.Setup(p => p.CreateLock("test-resource")).Returns(_lockMock.Object);
	}
}
