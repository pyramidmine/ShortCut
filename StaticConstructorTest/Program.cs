using System;
using System.Collections.Generic;

namespace StaticConstructorTest
{
	// 제네릭 타입 파라미터를 매 번 지정하기 번거로우면
	// using 구문을 사용해서 간단하게 사용할 수 있음
	using PacketFactory = PacketFactory<PacketKey>;

	internal struct PacketKey
	{
		public string CallType { get; set; }
		public string ReqNum { get; set; }
	}

	internal static class Program
	{
		public static void Main()
		{
			Console.WriteLine("Hello World!");

			// A1Today
			TestCreatePacket(new PacketKey{ CallType = "CPO1", ReqNum = "MP01_01_V01" });

			// A2Today
			TestCreatePacket(new PacketKey{ CallType = "CPO1", ReqNum = "MP02_01_V02" });

			// Unknown
			TestCreatePacket(new PacketKey{ CallType = "CPO1", ReqNum = "MP03_01_V01" });
		}

		private static void TestCreatePacket(PacketKey key)
		{
			Console.WriteLine($"Program.TestCreatePacket: key={key}");

			var packet = PacketFactory.CreatePacket(key);
			if (packet != null)
			{
				Console.WriteLine($"Packet created: key={key}, description={packet.Description}");
			}
			else
			{
				Console.WriteLine($"Can't find Packet key: key={key}");

				packet = new OraclePacket();
				Console.WriteLine($"Create default packet: description={packet.Description}");
			}
		}
	}

	/// <summary>
	/// 모든 패킷 클래스의 베이스 클래스
	/// </summary>
	/// <remarks>
	/// - 이 클래스를 직접 생성해서 사용하지 못하도록 abstract 클래스로 선언
	/// - 상속받는 클래스는 abstract 지정된 메서드와 속성 등을 필수적으로 구현해야 함
	/// </remarks>
	internal abstract class Packet
	{
		abstract public string Description { get; }
	}

	internal class A1TodayPacket : Packet
	{
		/// <summary>
		/// 패킷팩토리 클래스에서 이 클래스의 생성자를 호출해서 등록을 시도할 예정
		/// 그런데 패킷팩토리 클래스에서 호출할 때는 객체가 생성되지 않은 상태이므로 일반 생성자는 호출할 수 없음
		/// 반면, static 생성자는 객체를 생성하지 않은 상태에서도 호출 가능하므로 이 기능을 이용
		/// </summary>
		static A1TodayPacket()
		{
			Console.WriteLine("A1TodayPacket.sctor");
			PacketFactory.RegisterPacket(new PacketKey { CallType = "CPO1", ReqNum = "MP01_01_V01" }, typeof(A1TodayPacket));
		}

		public A1TodayPacket()
		{
			Console.WriteLine("A1TodayPacket.ctor");
		}

		override public string Description { get { return "A1Today Packet"; } }
	}

	internal class A2TodayPacket : Packet
	{
		static A2TodayPacket()
		{
			Console.WriteLine("A2TodayPacket.sctor");
			PacketFactory.RegisterPacket(new PacketKey{CallType = "CPO1", ReqNum = "MP02_01_V02"}, typeof(A2TodayPacket));
		}

		public A2TodayPacket()
		{
			Console.WriteLine("A2TodayPacket.ctor");
		}

		public override string Description => "A2Today Packet";
	}

	internal class OraclePacket: Packet
	{
		public override string Description => "Oracle Packet";
	}

	internal static class PacketFactory<K>
	{
		private static readonly Dictionary<K, Type> packets = new Dictionary<K, Type>();

		static PacketFactory()
		{
			Console.WriteLine("static PacketHandlerFactory()");
			var baseType = typeof(Packet);

			// 등록할 패킷 타입을 찾기 위해 모든 어셈블리 검색
			// 어셈블리는 DLL 파일 형태로 존재
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				// 어셈블리에 포함된 모든 타입을 검색
				foreach (var type in assembly.GetTypes())
				{
					// 패킷 타입을 상속 받았거나 또는 인터페이스를 포함하는 경우만 등록 허용
					if (IsSubclassOfOrHasInterface(type, baseType))
					{
						// 등록하려는 객체의 static 생성자를 호출
						// 해당 객체의 static 생성자는 패킷팩토리 메서드를 호출해서 각자 등록과정 수행
						System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
						Console.WriteLine($"PacketFactory.PacketFactory: {type.Name} class is registered.");
					}
				}
			}
		}

		/// <summary>
		/// 어떤 객체가 다른 객체를 상속받았거나, 인터페이스를 구현하는 지 확인
		/// </summary>
		/// <param name="subType">확인하려는 대상 객체</param>
		/// <param name="baseType">상위 객체 또는 인터페이스</param>
		/// <returns>테스트 대상 객체가 다른 객체를 상속받았거나 인터페이스를 구현하면 true, 아니면 false</returns>
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

		/// <summary>
		/// 패킷 클래스 등록
		/// </summary>
		/// <param name="key">패킷을 구분하는 키</param>
		/// <param name="type">등록하려는 패킷 타입</param>
		/// <remarks>
		/// - 패킷 키가 중복될 경우 Dictionary에 추가할 때 익셉션 발생
		/// - 개발 단계에서 모든 종류의 패킷을 사용한다면 등록되지 않은 패킷을 사용할 때 익셉션이 뜨겠지만,
		///   그렇지 않은 경우도 있으므로 익셉션이 발생하는 코드를 릴리즈 하는 건 위험
		/// - 유닛테스트에서 모든 패킷 등록이 중복 없이 됐는지 체크하는 게 좋을 것 같음
		/// </remarks>
		public static void RegisterPacket(K key, Type type)
		{
			Console.WriteLine("PacketFactory.RegisterPacket");
			try
			{
				#if RELEASE
				if (packets.ContainsKey(key))
				{
					return;
				}
				#endif

				packets.Add(key, type);
			}
			catch (Exception ex)
			{
				// 키가 중복됐을 경우 익셉션 발생!
				Console.WriteLine("================================================================================");
				Console.WriteLine($"EXCEPTION: {ex.GetType().Name}, {ex.Message}");
				Console.WriteLine("PacketFactory.RegisterPacket: FAILED!!!");
				Console.WriteLine("================================================================================");
			}
		}

		/// <summary>
		/// 패킷 키에 해당하는 패킷 객체 생성
		/// </summary>
		/// <param name="key">패킷 구분 키</param>
		/// <returns></returns>
		public static Packet CreatePacket(K key)
		{
			Console.WriteLine($"PacketFactory.CreatePacket: key={key}");
			if (packets.ContainsKey(key))
			{
				return (Packet)Activator.CreateInstance(packets[key]);
			}
			return default;
		}
	}
}
