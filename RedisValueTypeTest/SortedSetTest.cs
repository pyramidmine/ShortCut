using StackExchange.Redis;
using Xunit;

namespace RedisValueTypeTest
{
	public class SortedSetTest
	{
		[Fact]
		public void TestSortedSet()
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RedisServer.GetConnectionString());
			Assert.NotNull(redis);

			IDatabase db = redis.GetDatabase();
			Assert.NotNull(db);

			RedisKey key = "sortedset-test";

			RedisValue[] members = new RedisValue[] { "W00001", "W00002", "W00003", "W00004" };
			double[] scores = new double[] { 1, 3, 2, 4 };

			//
			// 멤버 모두 삭제
			//
			db.SortedSetRemove(key, members);

			//
			// 단일 멤버 추가, 스코어 변경 테스트
			//

			// 단일 멤버 추가
			bool result = db.SortedSetAdd(key, members[0], scores[0]);
			Assert.True(result);

			// 단일 멤버 스코어 변경 (이때 메서드의 리턴값은 false)
			scores[0] = 5;
			result = db.SortedSetAdd(key, members[0], scores[0]);
			Assert.False(result);

			// 멤버 스코어 값 확인
			double? actualScore = db.SortedSetScore(key, members[0]);
			Assert.True(actualScore != null);
			Assert.Equal(scores[0], actualScore);

			//
			// 다중 멤버 추가
			//
			for (int i = 1; i < members.Length; i++)
			{
				result = db.SortedSetAdd(key, members[i], scores[i]);
				Assert.True(result);
			}

			//
			// Rank로 검색
			//
			{
				RedisValue[] values = db.SortedSetRangeByRank(key);
				Assert.Equal(4, values.Length);
				Assert.Equal(members[2], values[0]);
				Assert.Equal(members[1], values[1]);
				Assert.Equal(members[3], values[2]);
				Assert.Equal(members[0], values[3]);
			}

			//
			// Score로 검색
			//
			{
				RedisValue[] values = db.SortedSetRangeByScore(key, scores[3]);
				Assert.Equal(2, values.Length);
				Assert.Equal(members[3], values[0]);
				Assert.Equal(members[0], values[1]);
			}
		}
	}
}
