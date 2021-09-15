using System;
using System.Collections.Generic;
using System.Linq;

namespace TupleKeyValueTest
{
	/// <summary>
	/// 클라이언트 정보
	/// </summary>
	public class ClientInfo : IComparable<ClientInfo>
	{
		// 예: "CPA", "CPT"
		public string DeviceType { get; set; }
		// 예: "T", "S"
		public string ServiceType { get; set; }
		// 예: "1.0", "2.0"
		public string ProgramVersion { get; set; }
		public ClientInfo(string deviceType, string serviceType, string programVersion)
		{
			DeviceType = deviceType;
			ServiceType = serviceType;
			ProgramVersion = programVersion;
		}
		/// <summary>
		/// ClientInfo 객체와 비교해서 같으면 0, 크면 양수, 작으면 음수를 리턴
		/// </summary>
		/// <param name="other">비교할 대상 ClientInfo</param>
		/// <returns>
		///		<para>0: DeviceType, ServiceType, ProgramVersion이 일치</para>
		///		<para>양수: DeviceType, ServiceType이 일치하고 ProgramVersion이 비교할 대상보다 높음</para>
		///		<para>음수: DeviceType, ServiceType이 일치하지 않거나, ProgramVersion이 비교할 대상보다 낮음</para>
		/// </returns>
		/// <remarks>
		/// <list type="bullet">
		///		<item>버전 비교할 때 소수점이 많으면 높은 버전으로 간주하므로, "1.0"보다 "1.0.0"이 크다고 판정하는 점에 주의!</item>
		/// </list>
		/// </remarks>
		public int CompareTo(ClientInfo other)
        {
            if (string.Compare(DeviceType, other.DeviceType, ignoreCase: true) == 0 &&
				string.Compare(ServiceType, other.ServiceType, ignoreCase: true) == 0)
			{
				return string.Compare(ProgramVersion, other.ProgramVersion);
			}
			else
			{
				return -1;
			}
        }
    }

	/// <summary>
	/// 서버 정보
	/// </summary>
	public class ServerInfo
	{
		public ClientInfo ClientInfo { get; set; }
		// 예: "168.126.63.1,7890"
		public string EndPoint { get; set; }
		public ServerInfo(ClientInfo clientInfo, string endPoint)
		{
			ClientInfo = clientInfo;
			EndPoint = endPoint;
		}
	}

	public class Program
	{
		/// <summary>
		/// 클라이언트 정보에 따른 접속할 서버 목록
		/// </summary>
		/// <remarks>
		///	<list type="bullet">
		///		<item>로직은 손 댈 필요 없으며 변경된 부분만 이 테이블에 반영하면 됨</item>
		///	</list>
		/// </remarks>
		private static readonly List<ServerInfo> ServerInfos = new List<ServerInfo>
		{
			/*
			new ServerInfo(new ClientInfo("CPA", "S", "1.1"), "10.0.1.1,7890"),
			new ServerInfo(new ClientInfo("CPA", "T", "1.1"), "192.168.0.1,8890"),
			new ServerInfo(new ClientInfo("CPA", "T", "1.2"), "192.168.0.2,8890"),
			new ServerInfo(new ClientInfo("CPT", "S", "1.2.0"), "10.1.2.0,7891"),
			new ServerInfo(new ClientInfo("CPT", "T", "1.2.0"), "192.168.1.1,8891"),
			new ServerInfo(new ClientInfo("CPT", "T", "2.0.0"), "192.168.1.1,8892"),
			*/

			// DeviceType, ServiceType, ProgramVersion, EndPoint 순서로 넣어줍니다
		}
		.OrderByDescending(x => x.ClientInfo)	// 검색할 때 최신 ClientInfo가 앞에 오도록 정렬합니다
		.ToList();

		static void Main(string[] args)
		{
			// 서버주소 목록 출력 (버전 높은 순서)
			Console.WriteLine("ServerInfo list:");
			foreach (ServerInfo item in ServerInfos)
			{
				Console.WriteLine($"ServerInfo: DeviceType={item.ClientInfo.DeviceType}, ServiceType={item.ClientInfo.ServiceType}, ProgramVersion={item.ClientInfo.ProgramVersion}, EndPoint={item.EndPoint}");
			}

			// 클라이언트 정보를 기준으로 서버엔드포인트 찾기
			Console.WriteLine();
			PrintFindServerEndPoint(new ClientInfo("CPA", "S", "1.2"));		// 10.0.1.1,7890
			PrintFindServerEndPoint(new ClientInfo("CPA", "S", "1.1"));     // 10.0.1.1,7890
			PrintFindServerEndPoint(new ClientInfo("CPA", "S", "1.0"));		// EMPTY
			PrintFindServerEndPoint(new ClientInfo("CPA", "T", "1.3"));		// 192.168.0.2,8890
			PrintFindServerEndPoint(new ClientInfo("CPA", "T", "1.2"));     // 192.168.0.2,8890
			PrintFindServerEndPoint(new ClientInfo("CPA", "T", "1.1"));     // 192.168.0.1,8890
			PrintFindServerEndPoint(new ClientInfo("CPA", "T", "1.0"));		// EMPTY
			PrintFindServerEndPoint(new ClientInfo("CPT", "S", "1.3.0"));	// 10.1.2.0,7891
			PrintFindServerEndPoint(new ClientInfo("CPT", "S", "1.2.0"));   // 10.1.2.0,7891
			PrintFindServerEndPoint(new ClientInfo("CPT", "S", "1.1.0"));	// EMPTY
		}
		
		private static string FindServerEndPoint(ClientInfo clientInfo)
		{
			// DeviceType, ServiceType은 일치해야 하고
			// ProgramVersion은 크거나 같아야 합니다.
			var item = ServerInfos
				.Where(x => 0 <= clientInfo.CompareTo(x.ClientInfo))
				.FirstOrDefault();
			
			return item?.EndPoint ?? string.Empty;
		}

		private static void PrintFindServerEndPoint(ClientInfo clientInfo)
		{
			Console.WriteLine($"ClientInfo: DeviceType={clientInfo.DeviceType}, ServiceType={clientInfo.ServiceType}, ProgramVersion={clientInfo.ProgramVersion}");
			string serverEndPoint = FindServerEndPoint(clientInfo);
			Console.WriteLine($"ServerEndPoint: {serverEndPoint}");
		}
	}
}
