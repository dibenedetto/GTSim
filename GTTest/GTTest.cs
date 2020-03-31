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
{
	public GTTest() : base(new GTAccident(5.0f, 10.0f, 1.0f, 1, 128, 128), 8086)
	{
		//this.Tick  += OnTick;
		//this.KeyUp += OnKeyUp;
	}

	DrivingVehicle dv = null;
	TrafficVehicle tv = null;

	private void OnTick(object sender, EventArgs e)
	{
		if (dv == null) return;

		dv.Update(TimeController.Now);
		tv.Update(TimeController.Now);

		//File.AppendAllText("sbuthre.txt", "speed : " + vehicle.Speed  + "\n");
		//File.AppendAllText("sbuthre.txt", "speed2: " + vehicle.Speed2 + "\n");

		if (Game.IsKeyPressed(Keys.NumPad8))
		{
			dv.Accelerate(1.0f);
		}
		else if (Game.IsKeyPressed(Keys.NumPad5))
		{
			dv.Brake(1.0f);
		}
		else
		{
			dv.Keep();
		}

		if (Game.IsKeyPressed(Keys.NumPad4))
		{
			dv.Steer(-1.0f);
		}
		else if (Game.IsKeyPressed(Keys.NumPad6))
		{
			dv.Steer(+1.0f);
		}
		else
		{
			dv.Steer(0.0f);
		}
	}

	private void OnKeyUp(object sender, KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
			case Keys.NumPad0:
				{
					Traffic.InitializeClearWorld();

					/*
					//Vector3 position   = Game.Player.Character.Position;
					//float   heading    = Game.Player.Character.Heading;
					Vector3 position   = new Vector3(-1306.281f, -2875.058f, 13.42174f);
					float   heading    = 58.0f;
					Model   model      = VehicleHash.Futo;
					float   maxSpeedMS = Constants.MAX_SPEED;

					vehicle?.Delete();
					vehicle = new DrivingVehicle("pluto", model, position, heading, false, maxSpeedMS);
					//vehicle.Thurst(10.0f);
					*/

					//if (false)
					{
						//Vector3 position   = Game.Player.Character.Position;
						//float   heading    = Game.Player.Character.Heading;
						Vector3 position = new Vector3(-1172.419f, -1346.944f, 2.0f);
						float   heading = 115.0f;
						Model   model = VehicleHash.Futo;
						bool    onStreet = false;
						float   maxSpeedMS = Constants.MAX_SPEED;

						dv = new DrivingVehicle("pluto", model, position, heading, onStreet, maxSpeedMS);
					}

					var rand = new Random();

					//if (false)
					{
						float maxDistance = 0.0f;
						float distance = maxDistance * 1.0f;
						float maxDrift = 4.0f;
						float drift = -maxDrift + (2.0f * maxDrift * ((float)(rand.NextDouble())));

						distance = 30.0f;
						drift    = 0.0f;

						Vector3 position = dv.Position + dv.Vehicle.ForwardVector * distance + dv.Vehicle.RightVector * drift;
						float heading = dv.Heading + 180.0f;
						Model model = VehicleHash.Futo;
						bool onStreet = false;
						float maxSpeedMS = Constants.MAX_SPEED;

						tv = new TrafficVehicle("pippo", model, position, heading, onStreet, maxSpeedMS);
					}
				}
				break;

			case Keys.NumPad1:
				{
					Vector3 position = Game.Player.Character.Position;
					File.AppendAllText("sbuthre.txt", "position: " + position.X + " -- " + position.Y + " -- " + position.Z + "\n");
				}
				break;

			default:
				break;
		}
	}
}
