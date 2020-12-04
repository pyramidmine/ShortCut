using System;
using System.Collections.Generic;
using System.Text;

namespace RedisValueTypeTest
{
	public sealed class RedisServer
	{
		public static string RedisServerEndPoint { get; set; } = "192.168.0.46:16379";
		public static string RedisServerPassword { get; set; } = "Ekswnr59";
		public static string GetConnectionString()
		{
			return $"{RedisServerEndPoint},allowAdmin=true,password={RedisServerPassword}";
		}
	}

	public class WorkerInfo
	{
		public string Name { get; set; }
		public string Latitude { get; set; }
		public string Longitude { get; set; }
		public string UpdateTime { get; set; }
		public string IsOnline { get; set; }
	}
}
