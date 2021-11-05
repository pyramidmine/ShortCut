using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace ConfigurationUnitTests
{
	public class ConfigurationTest
	{
		private readonly ITestOutputHelper output;

		public ConfigurationTest(ITestOutputHelper output)
		{
			this.output = output;
		}

		/// <summary>
		/// 환경변수 테스트
		/// </summary>
		/// <remarks>
		/// - 환경변수를 이용해서 런타임 환경에 따라 동작을 바꿀 수 있음
		///   예를 들어, SERVER_TYPE 환경변수가 REAL 이면 상용서버, STAGE 이면 스테이지서버 등으로 구분할 수 있음
		///   이것을 활용해서, appsettings.{env}.json 파일을 읽어들이면 서버 타입에 따라 알맞은 설정파일을 사용 가능
		/// </remarks>
		[Fact]
		public void TestGetEnvironmentVariable()
		{
			// 환경변수 조회
			var env = Environment.GetEnvironmentVariable("ALLUSERSPROFILE");
			Assert.Equal(@"C:\ProgramData", env);

			// 존재하지 않는 환경변수를 조회하면 null
			var none = Environment.GetEnvironmentVariable("FOOBAR");
			Assert.Null(none);
		}

		/// <summary>
		/// 환경변수를 컨피규레이션에 추가해서 조회
		/// </summary>
		/// <remarks>
		/// 환경변수도 컨피규레이션처럼 다룰 수 있음
		/// </remarks>
		[Fact]
		public void TestAddEnvironmentVariables()
		{
			var builder = new ConfigurationBuilder()
				.AddEnvironmentVariables();
			var config = builder.Build();

			Assert.Equal(@"C:\Program Files", config["ProgramFiles"]);

			// 존재하지 않는 항목을 조회하면 null
			// 리턴값이 null일뿐 조회하는 것 자체는 문제없음
			var none = config["FOOBAR"];
			Assert.Null(none);
		}

		/// <summary>
		/// 컨피규레이션 파일 읽기
		/// </summary>
		/// <remarks>
		/// - 컨피규레이션 파일에 경로가 따로 없을 경우, 실행파일과 같은 디렉터리에서 찾음
		/// - 개발할 때는 컨피규레이션 파일을 Output 디렉터리에 복사하는 설정을 켜 두어야 함
		/// - 파일 변경 후 다시 읽기 전까지 약간의 딜레이가 있음. ReloadDelay = 250 등으로 정의되므로 테스트 할 때는 이 시간을 감안해야 됨
		/// </remarks>
		[Fact]
		public void TestLoadConfigurationFile()
		{
			const string origConfigFilename = "appsettings.json";
			const string testConfigFilename = "appsettings.testLoadConfiguration.json";
			const string hostKey = "Hermes-Server:Host";
			const string portKey = "Hermes-Server:Port";
			const string hermesServerTag = "Hermes-Server";
			const string hostTag = "Host";
			const string portTag = "Port";

			// 원본 컨피규레이션 파일을 복사해서 테스트에 사용
			File.Copy(origConfigFilename, testConfigFilename, overwrite: true);

			// 컨피규레이션 파일 읽기
			// 이때, 파일이 변경되면 자동으로 다시 읽도록 reloadOnChange = true 설정
			var builder = new ConfigurationBuilder()
				.AddJsonFile(testConfigFilename, true, reloadOnChange: true);
			var config = builder.Build();

			var origServerSettings = new { Host = "0.0.0.0", Port = "37999" };

			// 다단계 키를 찾을 때는 중간에 콜론(:)을 넣어주면 됨
			Assert.Equal(origServerSettings.Host, config[hostKey]);
			Assert.Equal(origServerSettings.Port, config[portKey]);

			// 컨피규레이션 파일 변경
			var changedServerSettings = new { Host = "127.0.0.1", Port = "37998" };
			using (var fs = new FileStream(testConfigFilename, FileMode.Create))
			{
				using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
				{
					writer.WriteStartObject();
					writer.WriteStartObject(hermesServerTag);
					writer.WriteString(hostTag, changedServerSettings.Host);
					writer.WriteString(portTag, changedServerSettings.Port);
					writer.WriteEndObject();
					writer.WriteEndObject();
				}
				fs.Flush();
			}

			// ReloadDelay 시간이 있으므로 잠시 대기
			Thread.Sleep(500);

			// 변경된 설정이 반영되었는지 확인
			Assert.Equal(changedServerSettings.Host, config[hostKey]);
			Assert.Equal(changedServerSettings.Port, config[portKey]);

			// 설정파일을 원래대로 복원
			using (var fs = new FileStream(testConfigFilename, FileMode.Create))
			{
				using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
				{
					writer.WriteStartObject();
					writer.WriteStartObject(hermesServerTag);
					writer.WriteString(hostTag, origServerSettings.Host);
					writer.WriteString(portTag, origServerSettings.Port);
					writer.WriteEndObject();
					writer.WriteEndObject();
				}
				fs.Flush();
			}

			// ReloadDelay 시간이 있으므로 잠시 대기
			Thread.Sleep(500);

			// 원래 설정으로 돌아왔는지 확인
			Assert.Equal(origServerSettings.Host, config[hostKey]);
			Assert.Equal(origServerSettings.Port, config[portKey]);
		}

		/// <summary>
		/// 여러 개의 컨피규레이션 파일을 읽기
		/// </summary>
		[Fact]
		public void TestLoadMultipleConfigurationFiles()
		{
			const string appConfigFilename = "appSettings.json";
			const string packetConfigFilename = "packetSettings.json";
			const string hostKey = "Hermes-Server:Host";
			const string portKey = "Hermes-Server:Port";
			const string packetTypeKey = "PacketType";

			// 테스트 중에 만들어지는 파일이 이미 존재하면 삭제하고 시작
			if (File.Exists(packetConfigFilename))
			{
				File.Delete(packetConfigFilename);
			}

			// packetConfigFilename에 해당하는 파일은 없는 상태에서 여러 개의 컨피규레이션 파일 로드
			var builder = new ConfigurationBuilder()
				.AddJsonFile(appConfigFilename, true, true)
				.AddJsonFile(packetConfigFilename, true, true);
			var config = builder.Build();

			// 존재하는 컨피규레이션 파일은 정상 로드 되는지 확인
			var origServerSettings = new { Host = "0.0.0.0", Port = "37999" };
			Assert.Equal(origServerSettings.Host, config[hostKey]);
			Assert.Equal(origServerSettings.Port, config[portKey]);

			// 존재하지 않는 컨피규레이션 파일은 당연히 null
			Assert.Null(config[packetTypeKey]);

			// 존재하지 않는 컨피규레이션 파일을 런타임에 추가
			// 이때 자동으로 컨피규레이션을 읽어오는지 확인
			var packetSettings = new { PacketType = "Login" };

			using (var fs = new FileStream(packetConfigFilename, FileMode.Create))
			{
				using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
				{
					writer.WriteStartObject();
					writer.WriteString(packetTypeKey, packetSettings.PacketType);
					writer.WriteEndObject();
				}
				fs.Flush();
			}

			// ReloadDelay 시간이 있으므로 잠시 대기
			Thread.Sleep(500);

			// 추가된 설정이 자동으로 로드되었는지 확인
			Assert.Equal(packetSettings.PacketType, config[packetTypeKey]);

			// 추가된 설정 파일을 삭제
			File.Delete(packetConfigFilename);

			// ReloadDelay 시간이 있으므로 잠시 대기
			Thread.Sleep(500);

			// 삭제된 설정 파일의 설정이 null인지 확인
			Assert.Null(config[packetTypeKey]);
		}

		/// <summary>
		/// 와일드카드로 지정된 컨피규레이션 파일 읽기
		/// --> 불가능
		/// </summary>
		[Fact]
		public void TestWildcardConfigurationFiles()
		{
			const string packetConfigFilename = "wildcardSettings.json";
			const string wildcardConfigFilename = "wildcard*.json";
			const string packetTypeKey = "PacketType";

			// 테스트 중에 만들어지는 파일이 이미 존재하면 삭제하고 시작
			if (File.Exists(packetConfigFilename))
			{
				File.Exists(packetConfigFilename);
			}

			// 컨피규레이션 파일을 와일드카드로 지정해서 파일 로드
			var builder = new ConfigurationBuilder()
				.AddJsonFile(wildcardConfigFilename, true, true);
			var config = builder.Build();

			// 컨피규레이션 파일이 없으므로 해당하는 설정은 null인지 확인
			Assert.Null(config[packetTypeKey]);

			// 존재하지 않는 컨피규레이션 파일을 런타임에 생성
			// 이때 자동으로 컨피규레이션을 읽어오는지 확인
			// 와일드카드로 지정된 컨피규레이션이라는 게 차이
			var packetSettings = new { PacketType = "Login" };

			using (var fs = new FileStream(packetConfigFilename, FileMode.Create))
			{
				using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
				{
					writer.WriteStartObject();
					writer.WriteString(packetTypeKey, packetSettings.PacketType);
					writer.WriteEndObject();
				}
				fs.Flush();
			}

			// ReloadDelay 시간이 있으므로 잠시 대기
			Thread.Sleep(500);

			// 추가된 설정이 자동으로 로드되었는지 확인해 봤자 실패
			// 와일드카드로 지정된 파일은 감지하지 못하는 듯
			Assert.Null(config[packetTypeKey]);

			// 추가된 설정 파일을 삭제
			File.Delete(packetConfigFilename);

			// ReloadDelay 시간이 있으므로 잠시 대기
			Thread.Sleep(500);

			// 삭제된 설정 파일의 설정이 null인지 확인
			Assert.Null(config[packetTypeKey]);
		}

		/// <summary>
		/// 컨피규레이션이 변경됐을 때 메시지를 받을 수 있는지 확인
		/// </summary>
		[Fact]
		public void TestConfigurationChangedEvent()
		{
			const string configFilename = "testConfigurationChangedEvent.json";

			var builder = new ConfigurationBuilder()
				.AddJsonFile(configFilename, true, true)
				.AddEnvironmentVariables();
			var config = builder.Build();

			// 컨피규레이션 변경 이벤트를 받았는지 여부
			bool configChanged = false;

			// 설정파일이 변경됐다는 이벤트를 받을 콜백 설정
			ChangeToken.OnChange<IConfiguration>(
				() => config.GetReloadToken(),
				ConfigurationChangedCallback,
				config);

			// 컨피규레이션 변경
			using (var fs = new FileStream(configFilename, FileMode.Create))
			{
				using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
				{
					writer.WriteStartObject();
					writer.WriteString("HOMEPATH", "MyHomePath");
					writer.WriteEndObject();
				}
				fs.Flush();
			}

			// 컨피규레이션 변경 후 일정시간이 지나야 감지되므로 잠시 대기
			Thread.Sleep(500);

			Assert.True(configChanged);

			void ConfigurationChangedCallback(IConfiguration configuration)
			{
				System.Diagnostics.Debug.WriteLine($"TestConfigurationChangedEvent.ConfigurationChangedCallback called");
				configChanged = true;
			}
		}

		/// <summary>
		/// 런타임에 컨피규레이션 추가
		/// </summary>
		/// <remarks>
		/// - 현재는 없지만 나중에 추가될 지 모르는 컨피규레이션 파일을 와일드카드로 지정해서 감시해도 안 됨
		/// - 와일드카드는 안되므로 우회해야 함
		/// - 메인/서브 컨피규레이션으로 나눠서 메인 컨피규레이션은 서브 컨피규레이션 파일들의 목록을 저장
		/// - 서브 컨피규레이션 파일을 추가해야 하면 1) 서브 컨피규레이션 파일 추가 2) 메인 컨피규레이션에 있는 서브 컨피규레이션 목록 갱신
		/// </remarks>
		[Fact]
		public void TestAddConfigurationFileOnRuntime()
		{
			const string mainConfigFilename = "mainSettings.json";
			const string subConfigFilename = "subSettings.json";

			// 서브 컨피규레이션 파일은 테스트 전에 미리 삭제
			if (File.Exists(subConfigFilename))
			{
				File.Delete(subConfigFilename);
			}

			// 메인 컨피규레이션 설정
			// 일단, subSettingsOne.json 파일만 리스트에 있지만 실제로 파일은 없음
			using (var fs = new FileStream(mainConfigFilename, FileMode.Create))
			{
				using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
				{
					writer.WriteStartObject();
					writer.WriteStartArray("SubConfigFiles");
					writer.WriteEndArray();
					writer.WriteEndObject();
				}
				fs.Flush();
			}

			var builder = new ConfigurationBuilder()
				.AddJsonFile(mainConfigFilename);
			var config = builder.Build();

			// 현재 설정 상태 확인
			Assert.Null(config.GetSection("SubConfigFiles")?.Get<string[]>());
			Assert.Null(config["Packet,1,1,1:enabled"]);

			// 서브 컨피규레이션 파일을 만들고 설정항목을 넣기
			using (var fs = new FileStream(subConfigFilename, FileMode.Create))
			{
				using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
				{
					writer.WriteStartObject();
					writer.WriteStartObject("Packet,1,1,1");
					writer.WriteString("enabled", "true");
					writer.WriteEndObject();
					writer.WriteEndObject();
				}
				fs.Flush();
			}

			// 메인 컨피규레이션 파일에 서브 컨피규레이션 파일 추가
			using (var fs = new FileStream(mainConfigFilename, FileMode.Create))
			{
				using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
				{
					writer.WriteStartObject();
					writer.WriteStartArray("SubConfigFiles");
					writer.WriteStringValue(subConfigFilename);
					writer.WriteEndArray();
					writer.WriteEndObject();
				}
				fs.Flush();
			}

			// 메인 컨피규레이션 파일을 리로드 할 동안 잠시 대기
			Thread.Sleep(500);

			// 서브 컨피규레이션 파일을 컨피규레이션 빌더에 넣고 리빌드
			builder.AddJsonFile(subConfigFilename);
			config = builder.Build();

			// 메인 컨피규레이션 파일: 서브 컨피규레이션 파일이 잘 추가됐는지 확인
			Assert.Equal(subConfigFilename, config.GetSection("SubConfigFiles")?.Get<string[]>()?[0]);
			// 서브 컨피규레이션 파일: 설정항목이 잘 들어갔는지 확인
			Assert.Equal("true", config["Packet,1,1,1:enabled"]);
		}

		/// <summary>
		/// 메모리에 만든 컨피규레이션도 파일과 동일하게 설정하고 사용할 수 있는 지 테스트
		/// </summary>
		[Fact]
		public void Test_MemoryConfiguration()
		{
			const string sectionName = "ServerOptions";
			const string ip = "192.168.10.181";
			const int port = 8998;

			// 메모리에 Json 형식으로 컨피규레이션 작성
			string configContents = $"" +
				$"{{'{sectionName}':{{" +
					$"'IP':'{ip}', " +
					$"'Port':'{port}'}}" +
				$"}}";

			//
			// 메모리에 작성된 컨피규레이션 내용을 파일에 쓰고 읽기
			//
			var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Test_MemoryConfiguration.json");
			try
			{
				File.WriteAllText(configFilePath, configContents);
				var config = new ConfigurationBuilder()
					.AddJsonFile(configFilePath)
					.Build();
				ServerOptions so = new ServerOptions();
				config.Bind(sectionName, so);
				Assert.Equal(ip, so.IP);
				Assert.Equal(port, so.Port);
			}
			catch (Exception ex)
			{
				output.WriteLine($"Test_MemoryConfiguration, Exception:{ex.GetType()}, Message:{ex.Message}");
			}
			finally
			{
				if (File.Exists(configFilePath))
				{
					File.Delete(configFilePath);
				}
			}

			//
			// 메모리에 작성된 컨피규레이션 내용을 바로 읽기
			//
			try
			{
				using var ms = new MemoryStream(Encoding.UTF8.GetBytes(configContents));
				var config = new ConfigurationBuilder()
					.AddJsonStream(ms)
					.Build();
				ServerOptions so = new ServerOptions();
				config.Bind(sectionName, so);
				Assert.Equal(ip, so.IP);
				Assert.Equal(port, so.Port);
			}
			catch (Exception ex)
			{
				output.WriteLine($"Test_MemoryConfiguration, Exception:{ex.GetType()}, Message:{ex.Message}");
			}
		}

		/// <summary>
		/// 컨피규레이션 내용을 담을 클래스
		/// </summary>
		public class ServerOptions
		{
			public string IP { get; set; }
			public int Port { get; set; }
		};
	}
}
