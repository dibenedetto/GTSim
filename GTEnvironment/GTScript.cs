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

	DateTime      lastMessageTime = DateTime.MinValue;
	List<string>  receiveList     = null;
	DateTime      pingTime        = DateTime.MinValue;
	bool          pingSent        = false;

	public GTScript(GTEnvironment environment, int port = 8086)
	{
		this.environment = environment;
		if (this.environment == null)
		{
			this.environment = new GTEnvironment(10.0f, 10.0f, 1.0f, 1, 320, 240);
		}

		this.port   = port;
		this.buffer = new byte [8 * 1024];
		this.check  = new byte [1];

		this.receiveList = new List<string>();

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

	private void OnTick(object sender, EventArgs e)
	{
		if (!WaitForClient()) return;

		var message = (Dictionary<string, object>)(ReceiveMessage());
		if (message == null)
		{
			const double sendPingTime      = 2.0;
			const double respondToPingTime = 2.0;

			var now = DateTime.Now;

			if (pingSent)
			{
				var elapsed = (now - pingTime).TotalSeconds;
				if (elapsed > respondToPingTime)
				{
					File.AppendAllText("sbuthre.txt", "ping not answered\n");
					ApplyQuit();
				}
			}
			else
			{
				var elapsed = (now - lastMessageTime).TotalSeconds;
				if (elapsed > sendPingTime)
				{
					SendPing();
				}
			}

			return;
		}

		lastMessageTime = DateTime.Now;

		string code = ((string)(message["Code"]));
		object data = ((object)(message["Data"]));

		object result = null;

		//File.AppendAllText("sbuthre.txt", "code: " + code + "\n");

		switch (code)
		{
			case "Exit":
				{
					ApplyExit();
				}
				break;

			case "Quit":
				{
					ApplyQuit();
				}
				break;

			case "Pong":
				{
					ApplyPong();
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
					result = ApplyStep(data);
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

	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		;
	}

	private void OnKeyUp(object sender, KeyEventArgs e)
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
			ApplyQuit();
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
		if (receiveList.Count > 0)
		{
			//var    result = JsonSerializer.Deserialize<Dictionary<string, object>>(str);
			//File.AppendAllText("sbuthre.txt", "json: " + receiveList[0] + "\n");
			var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(receiveList[0]);
			receiveList.RemoveAt(0);
			return result;
		}

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

				string[] spearator = { "}{" };
				string[] strlist   = str.Split(spearator, StringSplitOptions.RemoveEmptyEntries);

				//File.AppendAllText("sbuthre.txt", "json-all: " + str + "\n");
				if (strlist.Length > 1)
				{
					strlist[0] = strlist[0] + "}";
					for (int i=1; i<(strlist.Length-1); ++i)
					{
						receiveList.Add("{" + strlist[i] + "}");
					}
					receiveList.Add("{" + strlist[strlist.Length - 1]);
				}

				//var    result = JsonSerializer.Deserialize<Dictionary<string, object>>(str);
				var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(strlist[0]);
				
				return result;
			}

			awaitingRead = null;
		}

		return null;
	}

	private object MakeMessage(string code, object data)
	{
		return new Dictionary<string, object>()
		{
			["Code"] = code,
			["Data"] = data
		};
	}

	private void SendPing()
	{
		var message = MakeMessage("Ping", null);
		SendMessage(message);

		pingSent = true;
		pingTime = DateTime.Now;
	}

	private void ApplyExit()
	{
		File.AppendAllText("sbuthre.txt", "apply exit\n");
		FinalizeServer();
		Abort();
	}

	private void ApplyQuit()
	{
		File.AppendAllText("sbuthre.txt", "apply quit\n");
		pingSent = false;
		stream   = null;
		client   = null;
		environment.Restart();
	}

	private void ApplyPong()
	{
		if (pingSent)
		{
			pingSent = false;
		}
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
		File.AppendAllText("sbuthre.txt", "apply reset\n");
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
