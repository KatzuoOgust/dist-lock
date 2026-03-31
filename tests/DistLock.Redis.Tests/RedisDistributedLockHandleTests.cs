using Moq;
using StackExchange.Redis;

namespace KatzuoOgust.DistLock;

public class RedisDistributedLockHandleTests
{
	private readonly Mock<IDatabase> _dbMock = new();

	private async Task<IDistributedLockHandle> AcquireHandleAsync()
	{
		_dbMock.Setup(db => db.StringSetAsync(
				It.IsAny<RedisKey>(),
				It.IsAny<RedisValue>(),
				It.IsAny<TimeSpan?>(),
				It.IsAny<When>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(true);

		var provider = new RedisDistributedLockProvider(_dbMock.Object, keyPrefix: "lock:");
		var handle = await provider.CreateLock("res").TryAcquireAsync(TimeSpan.FromSeconds(30));
		return handle!;
	}

	[Fact]
	public async Task ReleaseAsync_ExecutesReleaseScript()
	{
		var handle = await AcquireHandleAsync();
		_dbMock.Setup(db => db.ScriptEvaluateAsync(
				It.IsAny<string>(),
				It.IsAny<RedisKey[]>(),
				It.IsAny<RedisValue[]>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(RedisResult.Create((RedisValue)1L, null));

		await handle.ReleaseAsync();

		_dbMock.Verify(db => db.ScriptEvaluateAsync(
			It.IsAny<string>(),
			It.IsAny<RedisKey[]>(),
			It.IsAny<RedisValue[]>(),
			It.IsAny<CommandFlags>()), Times.Once());
	}

	[Fact]
	public async Task ReleaseAsync_SetsIsAcquiredFalse()
	{
		var handle = await AcquireHandleAsync();
		_dbMock.Setup(db => db.ScriptEvaluateAsync(
				It.IsAny<string>(),
				It.IsAny<RedisKey[]>(),
				It.IsAny<RedisValue[]>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(RedisResult.Create((RedisValue)1L, null));

		await handle.ReleaseAsync();

		Assert.False(handle.IsAcquired);
	}

	[Fact]
	public async Task ReleaseAsync_IsIdempotent()
	{
		var handle = await AcquireHandleAsync();
		_dbMock.Setup(db => db.ScriptEvaluateAsync(
				It.IsAny<string>(),
				It.IsAny<RedisKey[]>(),
				It.IsAny<RedisValue[]>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(RedisResult.Create((RedisValue)1L, null));

		await handle.ReleaseAsync();
		await handle.ReleaseAsync();

		_dbMock.Verify(db => db.ScriptEvaluateAsync(
			It.IsAny<string>(),
			It.IsAny<RedisKey[]>(),
			It.IsAny<RedisValue[]>(),
			It.IsAny<CommandFlags>()), Times.Once());
	}

	[Fact]
	public async Task ExtendAsync_ReturnsTrue_WhenScriptReturnsOne()
	{
		var handle = await AcquireHandleAsync();
		_dbMock.Setup(db => db.ScriptEvaluateAsync(
				It.IsAny<string>(),
				It.IsAny<RedisKey[]>(),
				It.IsAny<RedisValue[]>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(RedisResult.Create((RedisValue)1L, null));

		var extended = await handle.ExtendAsync(TimeSpan.FromSeconds(30));

		Assert.True(extended);
	}

	[Fact]
	public async Task ExtendAsync_ReturnsFalse_WhenScriptReturnsZero()
	{
		var handle = await AcquireHandleAsync();
		_dbMock.Setup(db => db.ScriptEvaluateAsync(
				It.IsAny<string>(),
				It.IsAny<RedisKey[]>(),
				It.IsAny<RedisValue[]>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(RedisResult.Create((RedisValue)0L, null));

		var extended = await handle.ExtendAsync(TimeSpan.FromSeconds(30));

		Assert.False(extended);
	}

	[Fact]
	public async Task ExtendAsync_ReturnsFalse_AfterRelease()
	{
		var handle = await AcquireHandleAsync();
		_dbMock.Setup(db => db.ScriptEvaluateAsync(
				It.IsAny<string>(),
				It.IsAny<RedisKey[]>(),
				It.IsAny<RedisValue[]>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(RedisResult.Create((RedisValue)1L, null));

		await handle.ReleaseAsync();
		var extended = await handle.ExtendAsync(TimeSpan.FromSeconds(30));

		Assert.False(extended);
	}

	[Fact]
	public async Task ExtendAsync_ThrowsArgumentOutOfRangeException_WhenExpiryIsNotPositive()
	{
		var handle = await AcquireHandleAsync();
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			handle.ExtendAsync(TimeSpan.Zero));
	}

	[Fact]
	public async Task DisposeAsync_ReleasesLock()
	{
		var handle = await AcquireHandleAsync();
		_dbMock.Setup(db => db.ScriptEvaluateAsync(
				It.IsAny<string>(),
				It.IsAny<RedisKey[]>(),
				It.IsAny<RedisValue[]>(),
				It.IsAny<CommandFlags>()))
			.ReturnsAsync(RedisResult.Create((RedisValue)1L, null));

		await handle.DisposeAsync();

		Assert.False(handle.IsAcquired);
		_dbMock.Verify(db => db.ScriptEvaluateAsync(
			It.IsAny<string>(),
			It.IsAny<RedisKey[]>(),
			It.IsAny<RedisValue[]>(),
			It.IsAny<CommandFlags>()), Times.Once());
	}
}

