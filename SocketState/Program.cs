using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketState
{
	internal static class Program
	{
		public static void Main()
		{
			Listener listener = new Listener(new IPEndPoint(IPAddress.Any, 31000), 100, AcceptCallback);
			Console.WriteLine($"Main, Call Listener.Listen()");
			listener.Listen();

			Thread.Sleep(1000);
			Console.WriteLine($"Main, Call Listener.Stop()");
			listener.Stop();

			Thread.Sleep(1000);
		}

		private static void AcceptCallback(object sender, SocketAsyncEventArgs saea)
		{
			Console.WriteLine($"Program.AcceptCallback, sender={sender}, saea={saea}");
		}
	}

	internal class Listener
	{
		private Socket listenSocket;
		private readonly IPEndPoint ipEndPoint;
		private readonly int backlog;
		private event EventHandler<SocketAsyncEventArgs> AcceptCallback;

		public Listener(IPEndPoint ipEndPoint, int backlog, EventHandler<SocketAsyncEventArgs> acceptCallback)
		{
			Console.WriteLine($"Listener.Listener, ipEndPoint={ipEndPoint}, backlog={backlog}");
			this.ipEndPoint = ipEndPoint;
			this.backlog = backlog;
			this.AcceptCallback += acceptCallback;
		}

		public void Listen()
		{
			Console.WriteLine("Listener.Listen");
			listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listenSocket.Bind(ipEndPoint);
			listenSocket.Listen(backlog);
			StartAccept(null);
		}

		public void Stop()
		{
			listenSocket?.Close();
			listenSocket = null;
		}

		private void StartAccept(SocketAsyncEventArgs saea)
		{
			Console.WriteLine($"Listener.StartAccept, saea={saea?.PrintInfo()}");
			if (saea == null)
			{
				saea = new SocketAsyncEventArgs();
				saea.Completed += AcceptCompleted;
			}
			else
			{
				saea.AcceptSocket = null;
			}

			bool pending = listenSocket.AcceptAsync(saea);
			if (!pending)
			{
				ProcessAccept(saea);
			}
		}

		private void AcceptCompleted(object sender, SocketAsyncEventArgs saea)
		{
			Console.WriteLine($"Listener.AcceptCompleted, sender={sender}, saea={saea?.PrintInfo()}");
			if (saea.SocketError == SocketError.Success)
			{
				ProcessAccept(saea);
			}
		}

		private void ProcessAccept(SocketAsyncEventArgs saea)
		{
			Console.WriteLine($"Listener.ProcessAccept, saea={saea?.PrintInfo()}");
			AcceptCallback?.Invoke(this, saea);
			StartAccept(saea);
		}
	}

	internal static class SocketAsyncEventArgsExtension
	{
		public static string PrintInfo(this SocketAsyncEventArgs saea)
		{
			return $"{{LastOperation={saea.LastOperation}, SocketError={saea.SocketError}}}";
		}
	}
}
