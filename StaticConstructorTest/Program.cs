using System;
using System.Collections.Generic;
using System.Reflection;

namespace StaticConstructorTest
{
	internal static class Program
	{
		public static void Main()
		{
			Console.WriteLine("Hello World!");

			Console.WriteLine("Create login packet...");
			var loginPacket = PacketFactory.CreatePacket(1);
			if (loginPacket != null)
			{
				Console.WriteLine($"Packet description: {loginPacket.Description}");
			}
			else
			{
				Console.WriteLine("Can't find login packet!");
			}
		}
	}

	internal abstract class Packet
	{
		abstract public string Description { get; }
	}

	internal class LoginPacket : Packet
	{
		static LoginPacket()
		{
			Console.WriteLine("LoginPacket.ctor");
			PacketFactory.RegisterPacket(1, typeof(LoginPacket));
		}

		override public string Description { get { return "Login Packet"; } }
	}

	internal static class PacketFactory
	{
		private static readonly Dictionary<int, Type> packets = new Dictionary<int, Type>();

		static PacketFactory()
		{
			Console.WriteLine("PacketFactory.PacketFactory");
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var baseType = typeof(Packet);
			foreach (var assembly in assemblies)
			{
				foreach (var type in assembly.GetTypes())
				{
					if (IsSubclassOfOrHasInterface(type, baseType))
					{
						Console.WriteLine($"PacketFactory.PacketFactory: {type.Name} class is registered.");
						System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
					}
				}
			}
		}

		private static bool IsSubclassOfOrHasInterface(Type subType, Type baseType)
		{
			if (baseType.IsInterface)
			{
				return subType.GetInterface(baseType.Name) != null;
			}
			else
			{
				return subType.IsSubclassOf(baseType);
			}
		}

		public static void RegisterPacket(int key, Type type)
		{
			Console.WriteLine("PacketFactory.RegisterPacket");
			packets.Add(key, type);
		}

		public static Packet CreatePacket(int key)
		{
			if (packets.ContainsKey(key))
			{
				var type = packets[key];
				return (Packet)Activator.CreateInstance(type.GetType());
			}
			return default;
		}
	}
}
