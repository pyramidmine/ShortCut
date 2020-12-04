using StackExchange.Redis;
using System;
using Xunit;

namespace RedisValueTypeTest
{
	public class StringTest
	{
		[Fact]
		public void TestString()
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RedisServer.GetConnectionString());
			Assert.NotNull(redis);

			IDatabase db = redis.GetDatabase();
			Assert.NotNull(db);

			const string key = "test:worker:name:w12345";
			db.KeyDelete(key);

			const string expectedValue = "Edward.MoonHwan.Lee";
			db.StringSet(key, expectedValue);

			string actualValue = db.StringGet(key);
			Assert.Equal(expectedValue, actualValue);
		}
	}
}
