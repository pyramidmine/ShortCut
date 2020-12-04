using System;
using System.Net;
using System.Net.Sockets;

namespace KeepAliveTestLibrary
{
	public class Connector
	{
		private readonly IPEndPoint ipEndPoint;
		private event EventHandler<SocketAsyncEventArgs> ConnectCallback;
		private Socket connectSocket;

		public Connector(IPEndPoint ipEndPoint, EventHandler<SocketAsyncEventArgs> connectCallback)
		{
			this.ipEndPoint = ipEndPoint;
			ConnectCallback += connectCallback;
		}

		public void Connect()
		{
			connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			StartConnect(null);
		}

		private void StartConnect(SocketAsyncEventArgs saea)
		{
			if (saea == null)
			{
				saea = new SocketAsyncEventArgs
				{
					RemoteEndPoint = ipEndPoint
				};
				saea.Completed += ConnectCompleted;
			}

			if (!connectSocket.ConnectAsync(saea))
			{
				ProcessConnect(saea);
			}
		}

		private void ConnectCompleted(object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				ProcessConnect(e);
			}
			else
			{
				Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ffff}] Connector.ConnectCompleted: {e.SocketError}");
			}
		}

		private void ProcessConnect(SocketAsyncEventArgs saea)
		{
			ConnectCallback?.Invoke(this, saea);
		}
	}
}
