using System;
using System.Collections.Generic;
using System.Text;

namespace ListTest
{
	internal static class Program
	{
		private static void Main()
		{
			TestStringListReference(false);
			TestListReference(false);
		}

		/// <summary>
		/// List 객체 참조 테스트
		/// </summary>
		/// <remarks>
		/// - 객체를 참조하는 경우, 원본 객체가 변경되면 참조하는 쪽에도 변경사항이 반영 됨
		/// - 원본 객체가 새로 할당되는 경우, 참조 객체는 원본 객체의 기존 값을 유지하므로 연결 관계는 끊어짐
		/// </remarks>
		/// <param name="enabled"></param>
		private static void TestListReference(bool enabled = true)
		{
			if (!enabled)
			{
				return;
			}

			// 원본 데이터
			byte[] data = Encoding.UTF8.GetBytes("12345");

			// 원본 데이터를 포함하는 패킷바디
			MockPacketBody orgPacketBody = new MockPacketBody { CompressionType = 0, Data = data };
			MockPacket orgPacket = new MockPacket { PacketType = 1, Bodies = new List<MockPacketBody> { orgPacketBody } };

			// 패킷 데이터를 그대로 참조하는 패킷바디 복사본
			MockPacket newPacket = new MockPacket { PacketType = 2, Bodies = orgPacket.Bodies };

			// 원본 패킷의 패킷바디 변경
			orgPacket.Bodies[0].Data = Encoding.UTF8.GetBytes("ABCDE");

			// 원본 패킷의 패킷바디 출력
			Console.WriteLine($"Original Packet, data:{Encoding.UTF8.GetString(orgPacket.Bodies[0].Data)}");
			// 클론 패킷의 패킷바디 출력
			Console.WriteLine($"Clone Packet, data:{Encoding.UTF8.GetString(newPacket.Bodies[0].Data)}");

			// 원본 패킷을 삭제
			orgPacket = null;

			// 클론 패킷의 패킷바디 출력
			Console.WriteLine($"Clone Packet, data:{Encoding.UTF8.GetString(newPacket.Bodies[0].Data)}");

			// 원본 패킷을 새로 할당
			orgPacket = new MockPacket { PacketType = 2, Bodies = new List<MockPacketBody> { new MockPacketBody { CompressionType = 0, Data = Encoding.UTF8.GetBytes("98765") } } };

			// 원본 패킷의 패킷바디 출력
			Console.WriteLine($"Original Packet, data:{Encoding.UTF8.GetString(orgPacket.Bodies[0].Data)}");
			// 클론 패킷의 패킷바디 출력
			Console.WriteLine($"Clone Packet, data:{Encoding.UTF8.GetString(newPacket.Bodies[0].Data)}");
		}

		private static void TestStringListReference(bool enabled = true)
		{
			if (!enabled)
			{
				return;
			}

			//
			// 리스트 A를 B에 Add 한 후 삭제하면 B에 Add 된 리스트는 어떻게 되나?
			//
			List<string> source = new List<string>
			{
				"Hello",
				"World"
			};
			List<List<string>> target = new List<List<string>>();

			target.Add(source);
			PrintStringLists(target);

			source.Add("!");
			PrintStringLists(target);

			source = new List<string>();
			source.Add("Welcome");
			PrintStringLists(target);

			AddPage(target, ref source);
			PrintStringLists(target);

			AddPage(target, ref source);
			PrintStringLists(target);
		}

		private static void PrintStringLists(List<List<string>> stringLists)
		{
			Console.WriteLine("====== PrintStringLists ======");
			foreach (var strs in stringLists)
			{
				foreach (var str in strs)
				{
					Console.WriteLine(str);
				}
			}
		}

		/// <summary>
		/// 파라미터로 넣어준 객체의 값을 바꿔주는 테스트
		/// </summary>
		/// <param name="pages"></param>
		/// <param name="records"></param>
		/// <remarks>
		/// - 일반 파라미터의 경우: records 값을 리셋해도 파라미터 변수 records가 리셋되는 거지 원본 객체가 리셋되는 게 아님
		/// - 레퍼런스 파라미터의 경우: records 파라미터가 원본을 가리키므로 records에 새 값을 할당하는 순간 원본도 새 값을 가리키게 됨
		/// </remarks>
		private static void AddPage(List<List<string>> pages, ref List<string> records)
		{
			pages.Add(records);
			records = new List<string>();
		}
	}

	internal class MockPacket
	{
		public int PacketType { get; set; }
		public List<MockPacketBody> Bodies { get; set; }
	}

	internal class MockPacketBody
	{
		public int CompressionType { get; set; }
		public byte[] Data { get; set; }
	}
}
