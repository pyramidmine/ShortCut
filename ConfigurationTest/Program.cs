using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConfigurationTest
{
	public static class Program
	{
		/// <summary>
		/// 리스트를 Bind 하면 Add로 동작해서 뒤에 계속 붙음 <br/>
		/// Bind 전에 Clear 해야 정상적으로 아이템들이 로드 됨
		/// </summary>
		private static List<Target> targets = new();

		/// <summary>
		/// 딕셔너리를 Bind 하면 Add로 동작하는데, 딕셔너리 특성상 기존 아이템은 삭제되고 새로 추가되는 효과 발생
		/// </summary>
		private static Dictionary<string, Receiver> receivers = new();

		public static void Main()
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appSettings.json", false, true)
				.Build();

			ChangeToken.OnChange<IConfiguration>(
				() => config.GetReloadToken(),
				OnConfigurationChanged,
				config);

			LoadConfiguration(config);

			Console.ReadLine();
		}

		private static void OnConfigurationChanged(IConfiguration config)
		{
			LoadConfiguration(config);
		}

		private static void LoadConfiguration(IConfiguration config)
		{
			targets.Clear();
			config?.GetSection("Targets")?.Bind(targets);
			PrintTargets(targets);

			config?.GetSection("Receivers")?.Bind(receivers);
			PrintReceivers(receivers);
		}

		private static void PrintTargets(List<Target> targetList)
		{
			Console.WriteLine("Targets");
			Console.WriteLine($"  HashCode:{targetList.GetHashCode()}");
			targetList.ForEach(target =>
			{
				Console.WriteLine("  Target");
				Console.WriteLine($"    HashCode:{target.GetHashCode()}");
				Console.WriteLine($"    {target}");
			});
		}

		private static void PrintReceivers(Dictionary<string, Receiver> receiverDictionary)
		{
			Console.WriteLine("Receivers");
			Console.WriteLine($"  HashCode:{receiverDictionary.GetHashCode()}");
			receiverDictionary.Values.ToList().ForEach(receiver =>
			{
				Console.WriteLine("  Receiver");
				Console.WriteLine($"    HashCode:{receiver.GetHashCode()}");
				Console.WriteLine($"    {receiver}");
			});
		}
	}

	public class Target
	{
		public string Name { get; set; }
		public string Drive { get; set; }
		public override string ToString()
		{
			return $"Name:{Name}, Drive:{Drive}";
		}
	}

	public class Receiver
	{
		public string Name { get; set; }
		public string PhoneNumber { get; set; }
		public override string ToString()
		{
			return $"Name:{Name}, PhoneNumber:{PhoneNumber}";
		}
	}
}
