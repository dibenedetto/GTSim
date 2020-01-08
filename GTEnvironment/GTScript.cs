using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

using JsonSerializer = System.Text.Json.JsonSerializer;

using GTA;
using GTA.Math;

using GTSim;

public abstract class GTScript : Script
{
	protected GTEnvironment environment = null;
	protected int           port        = 0;

	System.Threading.Tasks.Task<System.Net.Sockets.TcpClient> awaitingClient = null;

	TcpListener   server = null;
	TcpClient     client = null;
	NetworkStream stream = null;
	byte[]        buffer = null;

	public abstract void Implementable();

	public GTScript(GTEnvironment environment, int port = 8086)
	{
		this.environment = environment;
		if (this.environment == null)
		{
			this.environment = new GTEnvironment(10.0f, 10.0f, 4);
		}

		this.port   = port;
		this.buffer = new byte [8 * 1024];

		InitializeServer();

		this.KeyDown += OnKeyDown;
		this.KeyUp   += OnKeyUp  ;
		this.Tick    += OnTick   ;
	}

	public int Port
	{
		get { return port; }
	}

	public GTEnvironment Environment
	{
		get { return environment; }
	}

	public void OnTick(object sender, EventArgs e)
	{
		if (!WaitForClient()) return;

		var message = (Dictionary<string, object>)(ReceiveMessage());
		if (message == null) return;

		object result = null;

		switch ((string)(message["code"]))
		{
			case "quit":
				{
					ApplyQuit();
				}
				break;

			case "restart":
				{
					ApplyRestart();
				}
				break;

			case "explain":
				{
					result = ApplyExplain();
				}
				break;

			case "reset":
				{
					result = ApplyReset();
				}
				break;

			case "step":
				{
					result = ApplyStep((string)(message["data"]));
				}
				break;

			default:
				{
					;
				}
				break;
		}

		if (result != null)
		{
			SendMessage(result);
		}
	}

	public void OnKeyDown(object sender, KeyEventArgs e)
	{
		;
	}

	public void OnKeyUp(object sender, KeyEventArgs e)
	{
		;
	}

	private void InitializeServer()
	{
		FinalizeServer();

		server = new TcpListener(IPAddress.Any, port);
		server.Start(1);
	}

	private void FinalizeServer()
	{
		if (server != null)
		{
			server.Stop();
			server = null;
		}
	}

	private bool WaitForClient()
	{
		if (client != null)
		{
			if (client.Connected) return true;
			stream = null;
			client = null;
		}

		if (awaitingClient == null)
		{
			awaitingClient = server.AcceptTcpClientAsync();
		}

		if (awaitingClient.IsCompleted)
		{
			client         = awaitingClient.Result;
			awaitingClient = null;

			if (client != null)
			{
				stream = client.GetStream();
			}
		}

		return (client != null);
	}

	private void SendMessage(object message)
	{
		string str   = JsonSerializer.Serialize(message);
		byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
		stream.Write(bytes, 0, bytes.Length);
	}

	private object ReceiveMessage()
	{
		int    len = stream.Read(buffer, 0, buffer.Length);
		string str = System.Text.Encoding.ASCII.GetString(buffer, 0, len);
		var    result = JsonSerializer.Deserialize<Dictionary<string, object>>(str);
		return result;
	}

	private void ApplyQuit()
	{
		FinalizeServer();
		Abort();
	}

	private void ApplyRestart()
	{
		environment.Restart();
	}

	private object ApplyExplain()
	{
		Dictionary<string, object> result = new Dictionary<string, object>();
		result["StateDescriptors" ] = environment.StateDescriptors;
		result["ActionDescriptors"] = environment.ActionDescriptors;
		return result;
	}

	private object ApplyReset()
	{
		Result result = environment.Reset();
		return result;
	}

	private object ApplyStep(string action)
	{
		GTSim.Action act = JsonSerializer.Deserialize<GTSim.Action>(action);
		return environment.Step(act);
	}
}
