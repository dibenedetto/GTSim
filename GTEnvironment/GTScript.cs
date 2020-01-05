using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

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

	protected class GTMessage
	{
		public Dictionary<string, object> dict = new Dictionary<string, object>();
	}

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

	public void OnTick(object sender, EventArgs e)
	{
		if (!WaitForClient()) return;

		var message = ReceiveMessage();
		if (message == null) return;

		switch (message.dict["code"].ToString())
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

			case "reset":
				{
					var result = ApplyReset();
					SendMessage(result);
				}
				break;

			case "step":
				{
					var result = ApplyStep(message);
					SendMessage(result);
				}
				break;

			default:
				{
					;
				}
				break;
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

	private void SendMessage(GTMessage message)
	{
		//var str = message.
		;
	}

	private GTMessage ReceiveMessage()
	{
		var str = reader.ReadString();
		if (str == null) return null;

		return null;
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

	private GTMessage ApplyReset()
	{
		environment.Reset();
		return null;
	}

	private GTMessage ApplyStep(GTMessage action)
	{
		environment.Reset();
		return null;
	}
}
