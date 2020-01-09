using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
//using System.Text.Json;
using Newtonsoft.Json;

using GTA;
using GTA.Math;

using GTSim;

public abstract class GTScript : Script
{
	protected GTEnvironment environment = null;
	protected int           port        = 0;

	System.Threading.Tasks.Task<System.Net.Sockets.TcpClient> awaitingClient = null;
	System.Threading.Tasks.Task<int>                          awaitingRead   = null;

	TcpListener   server = null;
	TcpClient     client = null;
	NetworkStream stream = null;
	byte[]        buffer = null;
	byte[]        check  = null;


	public GTScript(GTEnvironment environment, int port = 8086)
	{
		this.environment = environment;
		if (this.environment == null)
		{
			this.environment = new GTEnvironment(10.0f, 10.0f, 4);
		}

		this.port   = port;
		this.buffer = new byte [8 * 1024];
		this.check  = new byte [1];

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

		switch ((string)(message["Code"]))
		{
			case "Quit":
				{
					ApplyQuit();
				}
				break;

			case "Explain":
				{
					result = ApplyExplain();
				}
				break;

			case "Reset":
				{
					result = ApplyReset();
				}
				break;

			case "Step":
				{
					result = ApplyStep(message["Action"]);
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
		if (e.KeyCode == Keys.NumPad0)
		{
			Game.Pause(false);
		}
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
			if (client.Connected)
			{
				return true;
			}
			stream = null;
			client = null;
			environment.Restart();
		}

		if (awaitingClient == null)
		{
			awaitingClient = server.AcceptTcpClientAsync();
		}

		if ((awaitingClient != null) && awaitingClient.IsCompleted)
		{
			if (!awaitingClient.IsCanceled && !awaitingClient.IsFaulted)
			{
				client = awaitingClient.Result;
				if (client != null)
				{
					stream = client.GetStream();
				}
			}

			awaitingClient = null;
			environment.Restart();
		}

		return (client != null);
	}

	private void SendMessage(object message)
	{
		//string str   = JsonSerializer.Serialize(message);
		string str   = JsonConvert.SerializeObject(message);
		byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);

		try
		{
			stream.WriteAsync(bytes, 0, bytes.Length);
		}
		catch
		{
			;
		}
	}

	private object ReceiveMessage()
	{
		if (awaitingRead == null)
		{
			try
			{
				awaitingRead = stream.ReadAsync(buffer, 0, buffer.Length);
			}
			catch
			{
				;
			}
		}

		if ((awaitingRead != null) && awaitingRead.IsCompleted)
		{
			if (!awaitingRead.IsCanceled && !awaitingRead.IsFaulted)
			{
				int len = awaitingRead.Result;
				awaitingRead = null;

				string str = System.Text.Encoding.ASCII.GetString(buffer, 0, len);
				//var    result = JsonSerializer.Deserialize<Dictionary<string, object>>(str);
				var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(str);
				return result;
			}

			awaitingRead = null;
		}

		return null;
	}

	private object MakeMessage(string code, object data)
	{
		Dictionary<string, object> message = new Dictionary<string, object>();
		message["Code"] = code;
		message["Data"] = data;
		return message;
	}

	private void ApplyQuit()
	{
		FinalizeServer();
		Abort();
	}

	private object ApplyExplain()
	{
		Dictionary<string, object> result = new Dictionary<string, object>();
		result["StateDescriptors" ] = environment.StateDescriptors;
		result["ActionDescriptors"] = environment.ActionDescriptors;
		return MakeMessage("Explain", result);
	}

	private object ApplyReset()
	{
		Result result = environment.Reset();
		return MakeMessage("Reset", result);
	}

	private object ApplyStep(object action)
	{
		//GTSim.Action act = JsonSerializer.Deserialize<GTSim.Action>(action);

		string       str = JsonConvert.SerializeObject(action);
		GTSim.Action act = JsonConvert.DeserializeObject<GTSim.Action>(str);
		object result = environment.Step(act);

		return MakeMessage("Step", result);
	}
}
