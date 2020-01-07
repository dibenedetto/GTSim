using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

using GTA;
using GTA.Math;

using GTSim;

public abstract class GTScript : Script
{
	protected int           port        = 0;
	protected GTEnvironment environment = null;

	TcpListener  server = null;
	TcpClient    client = null;
	BinaryReader reader = null;
	BinaryWriter writer = null;

	public abstract void Implementable();

	public GTScript(int port, GTEnvironment environment)
	{
		this.port = port;

		this.environment = environment;
		if (this.environment == null)
		{
			this.environment = new GTEnvironment(10.0f, 10.0f, 4);
		}

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
			client = null;
			return false;
		}

		client = server.AcceptTcpClient();
		if (client == null) return false;

		var stream = client.GetStream();
		reader = new BinaryReader(stream);
		writer = new BinaryWriter(stream);

		return true;
	}

	private void SendMessage(object message)
	{
		string str = JsonSerializer.Serialize(message);
		writer.Write(str);
	}

	private object ReceiveMessage()
	{
		var str = reader.ReadString();
		if (str == null) return null;
		var result = JsonSerializer.Deserialize<Dictionary<string, object>>(str);
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
		result["state_descriptors" ] = environment.StateDescriptors;
		result["action_descriptors"] = environment.ActionDescriptors;
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
