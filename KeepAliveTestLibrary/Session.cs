using MannaPlanet.MannaLog;
using System;
using System.Net.Sockets;
using System.Text;

namespace KeepAliveTestLibrary
{
	public class Session
	{
		public event EventHandler<SessionEventArgs> ReceiveCallback;
		public event EventHandler<SessionEventArgs> SendCallback;
		private readonly SocketAsyncEventArgs sendSaea = new SocketAsyncEventArgs();
		private readonly SocketAsyncEventArgs receiveSaea = new SocketAsyncEventArgs();
		private readonly Log logger;

		public Session(Socket socket, int socketBufferSize, Log logger)
		{
			sendSaea.AcceptSocket = socket;
			sendSaea.Completed += IOCompleted;
			sendSaea.SetBuffer(new byte[socketBufferSize], 0, socketBufferSize);

			receiveSaea.AcceptSocket = socket;
			receiveSaea.Completed += IOCompleted;
			receiveSaea.SetBuffer(new byte[socketBufferSize], 0, socketBufferSize);

			this.logger = logger;

			logger?.Info($"Session.Session: receiveSaea={receiveSaea.GetHashCode()}, sendSaea={sendSaea.GetHashCode()}, socket={receiveSaea.AcceptSocket.GetHashCode()}");
		}

		public void Start()
		{
			StartReceive();
		}

		public void Stop()
		{
			CloseSocket(receiveSaea);
			CloseSocket(sendSaea);
		}

		public void Send(byte[] src)
		{
			StartSend(src);
		}

		private void StartReceive()
		{
			bool pending = receiveSaea.AcceptSocket.ReceiveAsync(receiveSaea);
			if (!pending)
			{
				ProcessReceive(this, receiveSaea);
			}
		}

		private void StartSend(byte[] src)
		{
			int count = Math.Min(src.Length, sendSaea.Buffer.Length);
			Buffer.BlockCopy(src, 0, sendSaea.Buffer, 0, count);
			sendSaea.SetBuffer(0, count);

			if (!sendSaea.AcceptSocket.SendAsync(sendSaea))
			{
				ProcessSend(this, sendSaea);
			}
		}

		private void IOCompleted(object sender, SocketAsyncEventArgs e)
		{
			logger?.Info($"Session.IOCompleted: this={GetHashCode()}, e.SocketError={e?.SocketError}");

			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Receive:
					ProcessReceive(sender, e);
					break;
				case SocketAsyncOperation.Send:
					ProcessSend(sender, e);
					break;
				default:
					throw new ArgumentException($"[{DateTime.Now:HH:mm:ss.ffff}] The last operation completed on the socket was not a receive or send.");
			}
		}

		private void ProcessReceive(object sender, SocketAsyncEventArgs e)
		{
			logger?.Info($"Session.ProcessReceive: this={GetHashCode()}, e.SocketError={e?.SocketError}");

			if (e.SocketError == SocketError.Success && 0 < e.BytesTransferred)
			{
				ReceiveCallback?.Invoke(sender, new SessionEventArgs { Text = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred), BytesTransferred = e.BytesTransferred });
				StartReceive();
			}
			else
			{
				CloseSocket(e);
			}
		}

		private void ProcessSend(object sender, SocketAsyncEventArgs e)
		{
			logger?.Info($"Session.ProcessSend: this={GetHashCode()}, e.SocketError={e?.SocketError}");

			if (e.SocketError == SocketError.Success && 0 < e.BytesTransferred)
			{
				SendCallback?.Invoke(sender, new SessionEventArgs { BytesTransferred = e.BytesTransferred });
			}
			else
			{
				CloseSocket(e);
			}
		}

		private void CloseSocket(SocketAsyncEventArgs e)
		{
			logger?.Info($"Session.CloseSocket: this={GetHashCode()}, e.SocketError={e?.SocketError}");

			try
			{
				e.AcceptSocket.Shutdown(SocketShutdown.Both);
			}
			catch
			{
			}
			finally
			{
				e.AcceptSocket.Close();
			}
		}
	}

	public class SessionEventArgs
	{
		public string Text { get; set; }
		public int BytesTransferred { get; set; }
	}

}
