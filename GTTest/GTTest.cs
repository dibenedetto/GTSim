using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using GTA;
using GTA.Math;
using GTA.Native;

using GTSim;

public class GTTest : GTScript
//public class GTTest : Script
{
	public GTTest() : base(new GTAccident(10.0f, 10.0f, 0), 8086)
	{
		;
	}

	/*
	TimeController controller = null;
	Traffic        traffic    = null;

	public GTTest()
	{
		controller = new TimeController ();
		traffic    = new Traffic        ();
		
		KeyDown += OnKeyDown;
		KeyUp   += OnKeyUp;
		Tick    += OnTick;
	}

	public void OnTick(object sender, EventArgs e)
	{
		traffic.Update();

		{
			//int value = Game.GetControlValue(GTA.Control.LookLeftRight);
			//File.AppendAllText("sbuthre.txt", "value : " + value + "\n");
		}

		if (traffic.DrivingVehicle != null)
		{
			{
				float value = 0.0f;
				if (Game.IsKeyPressed(Keys.Y))
				{
					value = 0.3f;
				}
				traffic.DrivingVehicle.Thurst(value);
			}

			{
				float value = 0.0f;
				if (Game.IsKeyPressed(Keys.H))
				{
					value = 1.0f;
				}
				traffic.DrivingVehicle.Brake(value);
			}

			{
				float value = 0.0f;
				if (Game.IsKeyPressed(Keys.G))
				{
					value = -1.0f;
				}
				else if (Game.IsKeyPressed(Keys.J))
				{
					value = +1.0f;
				}
				traffic.DrivingVehicle.Steer(value);
			}
		}
	}

	public void OnKeyDown(object sender, KeyEventArgs e)
	{
		;
	}

	public void OnKeyUp(object sender, KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
			case Keys.NumPad0:
				{
					// vehicle
					Model   model    = VehicleHash.Futo;
					Vector3 position = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 5.0f;
					float   heading  = Game.Player.Character.Heading + 0.0f;
					float   maxSpeed = 150.0f * Constants.KMH_TO_MS;

					traffic.DrivingVehicle = new DrivingVehicle("stig", model, position, heading, true, maxSpeed);
				}
				break;

			case Keys.NumPad1:
				{
					traffic.InitializeRandomEpisode();
				}
				break;

			case Keys.NumPad2:
				{
					traffic.Clear();
				}
				break;

			case Keys.NumPad4:
				{
					traffic.Start();
				}
				break;

			case Keys.NumPad5:
				{
					traffic.Stop();
				}
				break;

			case Keys.NumPad3:
				{
					traffic.ActivateDrivingCamera();
				}
				break;

			case Keys.NumPad6:
				{
					traffic.ActivateTrafficCamera();
				}
				break;

			case Keys.NumPad9:
				{
					traffic.ActivateGameCamera();
				}
				break;

			case Keys.NumPad7:
				{
					controller.Reset();
				}
				break;

			case Keys.NumPad8:
				{
					const float fps = 10.0f;
					controller.Run(1.0f / fps);
				}
				break;

			default:
				{
					;
				}
				break;
		}
	}
	*/
}
