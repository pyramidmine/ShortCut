using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using Xunit;

namespace ConfigurationUnitTests
{
	public class ConfigurationTest
	{
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

			Assert.Equal(@"C:\ProgramData", config["ALLUSERSPROFILE"]);

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
			const string configFilename = "appsettings.json";
			const string hostKey = "Hermes-Server:Host";
			const string portKey = "Hermes-Server:Port";
			const string hermesServerTag = "Hermes-Server";
			const string hostTag = "Host";
			const string portTag = "Port";

			// 컨피규레이션 파일 읽기
			// 이때, 파일이 변경되면 자동으로 다시 읽도록 reloadOnChange = true 설정
			var builder = new ConfigurationBuilder()
				.AddJsonFile(configFilename, true, reloadOnChange: true);
			var config = builder.Build();

			var origServerSettings = new { Host = "0.0.0.0", Port = "37999" };

			// 다단계 키를 찾을 때는 중간에 콜론(:)을 넣어주면 됨
			Assert.Equal(origServerSettings.Host, config[hostKey]);
			Assert.Equal(origServerSettings.Port, config[portKey]);

			// 컨피규레이션 파일 변경
			var changedServerSettings = new { Host = "127.0.0.1", Port = "37998" };
			using (var fs = new FileStream(configFilename, FileMode.Create))
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
			using (var fs = new FileStream(configFilename, FileMode.Create))
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
		/// </summary>
		[Fact]
		public void TestWildcardConfigurationFiles()
		{

		}
	}
}
