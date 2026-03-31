using Moq;
using StackExchange.Redis;

namespace KatzuoOgust.DistLock.Redis;

public class RedisDistributedLockProviderTests
{
	private readonly Mock<IDatabase> _dbMock = new();

	[Fact]
	public void CreateLock_ReturnsLock_WithMatchingResource()
	{
		var provider = new RedisDistributedLockProvider(_dbMock.Object);

		IDistributedLock @lock = provider.CreateLock("my-resource");

		Assert.Equal("my-resource", @lock.Resource);
	}

	[Fact]
	public void Constructor_ThrowsArgumentNullException_WhenDatabaseIsNull()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new RedisDistributedLockProvider(null!));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public void CreateLock_ThrowsArgumentException_WhenResourceIsNullOrEmpty(string? resource)
	{
		var provider = new RedisDistributedLockProvider(_dbMock.Object);
		Assert.ThrowsAny<ArgumentException>(() => provider.CreateLock(resource!));
	}
}
