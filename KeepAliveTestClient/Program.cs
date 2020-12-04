using KeepAliveTestLibrary;
using MannaPlanet.MannaLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KeepAliveTestClient
{
	internal static class Program
	{
		private const int port = 34000;
		private const int socketBufferSize = 4096;
		private static Client client;
		private static Log logger;

		public static void Main()
		{
			logger = new LogBuilder().Build();

			logger?.Info("Client application starts...");

			string input;
			while ((input = Console.ReadLine()) != null)
			{
				if (input.StartsWith("quit", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
				else if (input.StartsWith("send ", StringComparison.OrdinalIgnoreCase))
				{
					if (client == null)
					{
						logger?.Warn("Client is not connected. First run 'start'.");
					}
					else
					{
						client.Send(Encoding.UTF8.GetBytes(input.Substring("send ".Length)));
					}
				}
				else if (input.StartsWith("start", StringComparison.OrdinalIgnoreCase))
				{
					if (client == null)
					{
						List<string> tokens = input.Split(' ').ToList();
						// start ip port
						if (tokens.Count != 3)
						{
							logger?.Warn("Parameters mismatch: Usage: start ip port");
							continue;
						}

						IPEndPoint ipEndPoint;
						try
						{
							ipEndPoint = new IPEndPoint(IPAddress.Parse(tokens[1]), int.Parse(tokens[2]));
							client = new Client(ipEndPoint, socketBufferSize, logger);
							client.Start();
						}
						catch (Exception ex)
						{
							logger?.Warn($"EXCEPTION: {ex.GetType().Name}, {ex.Message}");
						}
					}
					else
					{
						logger?.Warn("Client is connected already. Stop and restart.");
					}
				}
				else if (input.StartsWith("stop", StringComparison.OrdinalIgnoreCase))
				{
					if (client == null)
					{
						logger?.Warn("Client is not connected.");
					}
					else
					{
						client?.Stop();
					}
				}
			}
		}
	}

	internal class Client
	{
		private readonly IPEndPoint ipEndPoint;
		private readonly int socketBufferSize;
		private readonly Log logger;
		private Connector connector;
		private Session session;

		public Client(IPEndPoint ipEndPoint, int socketBufferSize, Log logger)
		{
			this.ipEndPoint = ipEndPoint;
			this.socketBufferSize = socketBufferSize;
			this.logger = logger;
		}

		public void Start()
		{
			connector = new Connector(ipEndPoint, ConnectCallback);
			connector.Connect();
		}

		public void Stop()
		{
			session?.Stop();
			session = null;
		}

		public void Send(byte[] src)
		{
			session?.Send(src);
		}

		private void ConnectCallback(object sender, SocketAsyncEventArgs e)
		{
			session = new Session(e.ConnectSocket, socketBufferSize, logger);
			logger?.Info($"Client.ConnectCallback: session={session.GetHashCode()}, e={e.GetHashCode()}, e.SocketError={e?.SocketError}");
			session.ReceiveCallback += ReceiveCallback;
			session.Start();
		}

		private void ReceiveCallback(object sender, SessionEventArgs e)
		{
			logger?.Info($"Client.ReceiveCallback, sender={sender.GetHashCode()}, e.BytesTransferred={e?.BytesTransferred}, e.Text={e?.Text}");
		}
	}
}
