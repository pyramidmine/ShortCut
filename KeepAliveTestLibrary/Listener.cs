using System;
using System.Net;
using System.Net.Sockets;

namespace KeepAliveTestLibrary
{
	public class Listener
	{
		private readonly IPEndPoint ipEndPoint;
		private readonly int backlog;
		private event EventHandler<SocketAsyncEventArgs> AcceptCallback;
		private Socket listenSocket;

		public Listener(IPEndPoint ipEndPoint, int backlog, EventHandler<SocketAsyncEventArgs> acceptCallback)
		{
			this.ipEndPoint = ipEndPoint;
			this.backlog = backlog;
			AcceptCallback += acceptCallback;
		}

		public void Listen()
		{
			listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listenSocket.Bind(ipEndPoint);
			listenSocket.Listen(backlog);
			StartAccept(null);
		}

		private void StartAccept(SocketAsyncEventArgs saea)
		{
			if (saea == null)
			{
				saea = new SocketAsyncEventArgs();
				saea.Completed += AcceptCompleted;
			}
			else
			{
				saea.AcceptSocket = null;
			}

			if (!listenSocket.AcceptAsync(saea))
			{
				ProcessAccept(saea);
			}
		}

		private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				ProcessAccept(e);
			}
			else
			{
				Console.WriteLine($"Server.AcceptCompleted: {e.SocketError}");
			}
		}

		private void ProcessAccept(SocketAsyncEventArgs saea)
		{
			AcceptCallback?.Invoke(this, saea);
			StartAccept(saea);
		}
	}
}
