using StackExchange.Redis;
using Xunit;

namespace RedisValueTypeTest
{
	public class TransactionTest
	{
		/// <summary>
		/// Redis에서 트랜잭션 처리가 가능할 지 테스트
		/// </summary>
		[Fact]
		public void TestTransaction()
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RedisServer.GetConnectionString());
			Assert.NotNull(redis);

			IDatabase db = redis.GetDatabase();
			Assert.NotNull(db);

			// '배송원 정보'와 '배송원 정보 동기화 시간'은 키가 따로 있음

			// '배송원 정보 동기화 시간'을 먼저 조회한 다음,
			// 이 값을 기준으로 '배송원 정보'를 얻으려고 하는데,
			// 이때 다른 서버 또는 쓰레드에서 먼저 배송원 정보를 업데이트 해 버리면?

			// 해결책은 트랜잭션 처리를 도입하는 것

			var tran = db.CreateTransaction();
		}
	}
}
