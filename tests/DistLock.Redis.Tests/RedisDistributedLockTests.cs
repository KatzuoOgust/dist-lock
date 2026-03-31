using Moq;
using StackExchange.Redis;

namespace KatzuoOgust.DistLock;

public class RedisDistributedLockTests
{
	private readonly Mock<IDatabase> _dbMock = new();

	private RedisDistributedLockProvider Provider => new(_dbMock.Object, keyPrefix: "lock:");

	[Fact]
	public async Task TryAcquireAsync_ReturnsHandle_WhenKeySetSucceeds()
	{
		_dbMock.Setup(db => db.StringSetAsync(
				It.IsAny<RedisKey>(),
				It.IsAny<RedisValue>(),
				It.IsAny<TimeSpan?>(),
				It.IsAny<When>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(true);

		IDistributedLockHandle? handle = await Provider.CreateLock("res")
			.TryAcquireAsync(TimeSpan.FromSeconds(30));

		Assert.NotNull(handle);
		Assert.Equal("res", handle.Resource);
		Assert.True(handle.IsAcquired);
	}

	[Fact]
	public async Task TryAcquireAsync_ReturnsNull_WhenKeySetFails()
	{
		_dbMock.Setup(db => db.StringSetAsync(
				It.IsAny<RedisKey>(),
				It.IsAny<RedisValue>(),
				It.IsAny<TimeSpan?>(),
				It.IsAny<When>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(false);

		IDistributedLockHandle? handle = await Provider.CreateLock("res")
			.TryAcquireAsync(TimeSpan.FromSeconds(30));

		Assert.Null(handle);
	}

	[Fact]
	public async Task TryAcquireAsync_SetsKeyWithPrefixedResource()
	{
		_dbMock.Setup(db => db.StringSetAsync(
				It.IsAny<RedisKey>(),
				It.IsAny<RedisValue>(),
				It.IsAny<TimeSpan?>(),
				It.IsAny<When>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(true);

		await Provider.CreateLock("my-resource").TryAcquireAsync(TimeSpan.FromSeconds(10));

		_dbMock.Verify(db => db.StringSetAsync(
			"lock:my-resource",
			It.IsAny<RedisValue>(),
			TimeSpan.FromSeconds(10),
			When.NotExists,
			CommandFlags.None), Times.Once());
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task TryAcquireAsync_ThrowsArgumentOutOfRangeException_WhenExpiryIsNotPositive(int expirySeconds)
	{
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			Provider.CreateLock("res").TryAcquireAsync(TimeSpan.FromSeconds(expirySeconds)));
	}
}
