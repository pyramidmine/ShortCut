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
		/// ȯ�溯�� �׽�Ʈ
		/// </summary>
		/// <remarks>
		/// - ȯ�溯���� �̿��ؼ� ��Ÿ�� ȯ�濡 ���� ������ �ٲ� �� ����
		///   ���� ���, SERVER_TYPE ȯ�溯���� REAL �̸� ��뼭��, STAGE �̸� ������������ ������ ������ �� ����
		///   �̰��� Ȱ���ؼ�, appsettings.{env}.json ������ �о���̸� ���� Ÿ�Կ� ���� �˸��� ���������� ��� ����
		/// </remarks>
		[Fact]
		public void TestGetEnvironmentVariable()
		{
			var env = Environment.GetEnvironmentVariable("ALLUSERSPROFILE");
			Assert.Equal(@"C:\ProgramData", env);

			// �������� �ʴ� ȯ�溯���� ��ȸ�ϸ� null
			var none = Environment.GetEnvironmentVariable("FOOBAR");
			Assert.Null(none);
		}

		/// <summary>
		/// ȯ�溯���� ���ǱԷ��̼ǿ� �߰��ؼ� ��ȸ
		/// </summary>
		/// <remarks>
		/// ȯ�溯���� ���ǱԷ��̼�ó�� �ٷ� �� ����
		/// </remarks>
		[Fact]
		public void TestAddEnvironmentVariables()
		{
			var builder = new ConfigurationBuilder()
				.AddEnvironmentVariables();
			var config = builder.Build();

			Assert.Equal(@"C:\ProgramData", config["ALLUSERSPROFILE"]);

			// �������� �ʴ� �׸��� ��ȸ�ϸ� null
			// ���ϰ��� null�ϻ� ��ȸ�ϴ� �� ��ü�� ��������
			var none = config["FOOBAR"];
			Assert.Null(none);
		}

		/// <summary>
		/// ���ǱԷ��̼� ���� �б�
		/// </summary>
		/// <remarks>
		/// - ���ǱԷ��̼� ���Ͽ� ��ΰ� ���� ���� ���, �������ϰ� ���� ���͸����� ã��
		/// - ������ ���� ���ǱԷ��̼� ������ Output ���͸��� �����ϴ� ������ �� �ξ�� ��
		/// - ���� ���� �� �ٽ� �б� ������ �ణ�� �����̰� ����. ReloadDelay = 250 ������ ���ǵǹǷ� �׽�Ʈ �� ���� �� �ð��� �����ؾ� ��
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

			// ���ǱԷ��̼� ���� �б�
			// �̶�, ������ ����Ǹ� �ڵ����� �ٽ� �е��� reloadOnChange = true ����
			var builder = new ConfigurationBuilder()
				.AddJsonFile(configFilename, true, reloadOnChange: true);
			var config = builder.Build();

			var origServerSettings = new { Host = "0.0.0.0", Port = "37999" };

			// �ٴܰ� Ű�� ã�� ���� �߰��� �ݷ�(:)�� �־��ָ� ��
			Assert.Equal(origServerSettings.Host, config[hostKey]);
			Assert.Equal(origServerSettings.Port, config[portKey]);

			// ���ǱԷ��̼� ���� ����
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

			// ReloadDelay �ð��� �����Ƿ� ��� ���
			Thread.Sleep(500);

			// ����� ������ �ݿ��Ǿ����� Ȯ��
			Assert.Equal(changedServerSettings.Host, config[hostKey]);
			Assert.Equal(changedServerSettings.Port, config[portKey]);

			// ���������� ������� ����
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

			// ReloadDelay �ð��� �����Ƿ� ��� ���
			Thread.Sleep(500);

			// ���� �������� ���ƿԴ��� Ȯ��
			Assert.Equal(origServerSettings.Host, config[hostKey]);
			Assert.Equal(origServerSettings.Port, config[portKey]);
		}

		/// <summary>
		/// ���� ���� ���ǱԷ��̼� ������ �б�
		/// </summary>
		[Fact]
		public void TestLoadMultipleConfigurationFiles()
		{
			const string appConfigFilename = "appSettings.json";
			const string packetConfigFilename = "packetSettings.json";
			const string hostKey = "Hermes-Server:Host";
			const string portKey = "Hermes-Server:Port";
			const string packetTypeKey = "PacketType";

			// packetConfigFilename�� �ش��ϴ� ������ ���� ���¿��� ���� ���� ���ǱԷ��̼� ���� �ε�
			var builder = new ConfigurationBuilder()
				.AddJsonFile(appConfigFilename, true, true)
				.AddJsonFile(packetConfigFilename, true, true);
			var config = builder.Build();

			// �����ϴ� ���ǱԷ��̼� ������ ���� �ε� �Ǵ��� Ȯ��
			var origServerSettings = new { Host = "0.0.0.0", Port = "37999" };
			Assert.Equal(origServerSettings.Host, config[hostKey]);
			Assert.Equal(origServerSettings.Port, config[portKey]);

			// �������� �ʴ� ���ǱԷ��̼� ������ �翬�� null
			Assert.Null(config[packetTypeKey]);

			// �������� �ʴ� ���ǱԷ��̼� ������ ��Ÿ�ӿ� �߰�
			// �̶� �ڵ����� ���ǱԷ��̼��� �о������ Ȯ��
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

			// ReloadDelay �ð��� �����Ƿ� ��� ���
			Thread.Sleep(500);

			// �߰��� ������ �ڵ����� �ε�Ǿ����� Ȯ��
			Assert.Equal(packetSettings.PacketType, config[packetTypeKey]);

			// �߰��� ���� ������ ����
			File.Delete(packetConfigFilename);

			// ReloadDelay �ð��� �����Ƿ� ��� ���
			Thread.Sleep(500);

			// ������ ���� ������ ������ null���� Ȯ��
			Assert.Null(config[packetTypeKey]);
		}

		/// <summary>
		/// ���ϵ�ī��� ������ ���ǱԷ��̼� ���� �б�
		/// </summary>
		[Fact]
		public void TestWildcardConfigurationFiles()
		{

		}
	}
}
