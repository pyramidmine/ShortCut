using StackExchange.Redis;
using System;
using System.Collections.Generic;
using Xunit;

namespace RedisValueTypeTest
{
	public class HashTest
	{
		private readonly WorkerInfo wi = new WorkerInfo
		{
			Name = "홍길동",
			Longitude = "127.093294",
			Latitude = "37.098214",
			UpdateTime = $"{DateTime.Now:yyyyMMddhhmmss}",
			IsOnline = "N"
		};

		[Fact]
		public void TestHash()
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RedisServer.GetConnectionString());
			Assert.NotNull(redis);

			IDatabase db = redis.GetDatabase();
			Assert.NotNull(db);

			// 테스트를 위해 일단 키를 삭제
			const string key = "test:worker:info:w12345";
			db.KeyDelete(key);

			List<HashEntry> expectedValues = new List<HashEntry>
			{
				new HashEntry("Name", wi.Name),
				new HashEntry("LocationX", wi.Longitude),
				new HashEntry("LocationY", wi.Latitude),
				new HashEntry("UpdateTime", wi.UpdateTime),
				new HashEntry("IsOnline", wi.IsOnline)
			};

			// 해시 엔트리 추가
			db.HashSet(key, expectedValues.ToArray());

			// 해시 엔트리 획득
			HashEntry[] actualValues = db.HashGetAll(key);
			Assert.Equal(expectedValues, actualValues);

			// 특정 해시 엔트리 값 변경
			expectedValues[3] = new HashEntry("UpdateTime", $"{DateTime.Now:yyyyMMddhhmmss}");
			db.HashSet(key, "UpdateTime", expectedValues[3].Value);
			actualValues = db.HashGetAll(key);
			Assert.Equal(expectedValues, actualValues);

			// 특정 해시 엔트리 삭제
			expectedValues.RemoveAt(0);
			db.HashDelete(key, "Name");
			actualValues = db.HashGetAll(key);
			Assert.Equal(expectedValues, actualValues);
		}
	}
}
