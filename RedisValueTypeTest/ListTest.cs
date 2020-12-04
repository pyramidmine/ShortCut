using StackExchange.Redis;
using System.Collections.Generic;
using Xunit;

namespace RedisValueTypeTest
{
	public class ListTest
	{
		private readonly List<RedisValue> expectedValues = new List<RedisValue>
		{
			"Cash", "CreditCard", "BankTransfer", "MannaPoint"
		};

		[Fact]
		public void TestList()
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RedisServer.GetConnectionString());
			Assert.NotNull(redis);

			IDatabase db = redis.GetDatabase();
			Assert.NotNull(db);

			// 테스트를 위해 일단 키를 삭제
			const string key = "test:worker:payments:w12345";
			db.KeyDelete(key);

			// 리스트 값 추가
			db.ListRightPush(key, expectedValues.ToArray());

			// 리스트 값 획득해서 검증
			RedisValue[] actualValues = db.ListRange(key);
			Assert.Equal(expectedValues, actualValues);

			// 리스트 왼쪽 아이템 삭제 및 추가
			RedisValue item = db.ListLeftPop(key);
			db.ListLeftPush(key, item);
			actualValues = db.ListRange(key);
			Assert.Equal(expectedValues, actualValues);

			// 리스트 오른쪽에 아이템 추가
			expectedValues.Add("None");
			db.ListRightPush(key, "None");
			actualValues = db.ListRange(key);
			Assert.Equal(expectedValues, actualValues);

			// 특정 아이템 삭제
			expectedValues.Remove("MannaPoint");
			db.ListRemove(key, "MannaPoint");
			actualValues = db.ListRange(key);
			Assert.Equal(expectedValues, actualValues);
		}
	}
}
