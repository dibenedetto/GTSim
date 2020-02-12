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
	public GTTest() : base(new GTAccident(5.0f, 10.0f, 1.0f, 1, 64, 64), 8086)
	{
		//this.Tick  += OnTick;
		//this.KeyUp += OnKeyUp;
	}

	DrivingVehicle vehicle = null;

	private void OnTick(object sender, EventArgs e)
	{
		if (Game.IsKeyPressed(Keys.NumPad9))
		{
			File.AppendAllText("sbuthre.txt", "position: " + Game.Player.Character.Position + "\n");
		}

		if (vehicle == null) return;

		Traffic.UpdateClearWorld();

		vehicle.Update(TimeController.Now);

		//File.AppendAllText("sbuthre.txt", "speed : " + vehicle.Speed  + "\n");
		//File.AppendAllText("sbuthre.txt", "speed2: " + vehicle.Speed2 + "\n");

		if (Game.IsKeyPressed(Keys.NumPad8))
		{
			vehicle.Accelerate(1.0f);
		}
		else if (Game.IsKeyPressed(Keys.NumPad5))
		{
			vehicle.Brake(1.0f);
		}
		else
		{
			//vehicle.Keep();
		}

		if (Game.IsKeyPressed(Keys.NumPad4))
		{
			vehicle.Steer(-1.0f);
		}
		else if (Game.IsKeyPressed(Keys.NumPad6))
		{
			vehicle.Steer(+1.0f);
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
						Vector3 position = new Vector3(-1306.281f, -2875.058f, 13.42174f);
						float   heading = 58.0f;
						Model   model = VehicleHash.Futo;
						bool    onStreet = false;
						float   maxSpeedMS = Constants.MAX_SPEED;

						var dv = new DrivingVehicle("pluto", model, position, heading, onStreet, maxSpeedMS);
						vehicle = dv;
					}

					var rand = new Random();

					//if (false)
					{
						float maxDistance = 0.0f;
						float distance = maxDistance * 1.0f;
						float maxDrift = 4.0f;
						float drift = -maxDrift + (2.0f * maxDrift * ((float)(rand.NextDouble())));
						drift = maxDrift;

						Vector3 position = vehicle.Position + vehicle.Vehicle.ForwardVector * distance + vehicle.Vehicle.RightVector * drift;
						float heading = vehicle.Heading + 180.0f;
						Model model = VehicleHash.Futo;
						bool onStreet = false;
						float maxSpeedMS = Constants.MAX_SPEED;

						var tv = new TrafficVehicle("pippo", model, position, heading, onStreet, maxSpeedMS);
					}
				}
				break;

			default:
				break;
		}
	}
}
