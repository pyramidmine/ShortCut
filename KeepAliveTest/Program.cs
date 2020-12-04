using KeepAliveTestLibrary;
using MannaPlanet.MannaLog;
using System;
using System.Net;
using System.Net.Sockets;

namespace KeepAliveTest
{
	internal static class Program
	{
		private const int port = 34000;
		private const int backlog = 100;
		private const int socketBufferSize = 4096;
		private static Server server;
		private static Log logger;

		private static void Main()
		{
			logger = new LogBuilder().Build();

			logger?.Info($"Server starts: IP={IPAddress.Any}, port={port}");
			server = new Server(new IPEndPoint(IPAddress.Any, port), backlog, socketBufferSize, logger);
			server.Start();

			logger?.SetFileMinLevel(Serilog.Events.LogEventLevel.Warning);

			string input;
			while ((input = Console.ReadLine()) != null)
			{
				switch (input.ToLower())
				{
					case "quit":
						return;
					case "close":
						server.Stop();
						break;
				}
			}
		}
	}

	internal class Server
	{
		private readonly Listener listener;
		private readonly int socketBufferSize;
		private readonly Log logger;
		private Session session;

		public Server(IPEndPoint ipEndPoint, int backlog, int socketBufferSize, Log logger)
		{
			this.socketBufferSize = socketBufferSize;
			listener = new Listener(ipEndPoint, backlog, AcceptCallback);
			this.logger = logger;
		}

		public void Start()
		{
			listener.Listen();
		}

		public void Stop()
		{
			session?.Stop();
			session = null;
		}

		private void AcceptCallback(object sender, SocketAsyncEventArgs e)
		{
			session = new Session(e.AcceptSocket, socketBufferSize, logger);
			session.ReceiveCallback += ReceiveCallback;
			logger?.Info($"Server.AcceptCallback, sender={sender.GetHashCode()}, e.SocketError={e?.SocketError}");
			session.Start();
		}

		private void ReceiveCallback(object sender, SessionEventArgs e)
		{
			logger?.Info($"Server.ReceiveCallback, sender={sender.GetHashCode()}, e.BytesTransferred={e?.BytesTransferred}, e.Text={e?.Text}");
		}
	}
}
