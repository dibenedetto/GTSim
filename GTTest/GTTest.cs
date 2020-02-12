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
		if (vehicle == null) return;

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
			vehicle.Keep();
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
					Vector3 position   = Game.Player.Character.Position;
					float   heading    = Game.Player.Character.Heading;
					Model   model      = VehicleHash.Futo;
					float   maxSpeedMS = Constants.MAX_SPEED;

					vehicle?.Delete();
					vehicle = new DrivingVehicle("pluto", model, position, heading, true, maxSpeedMS);
					vehicle.Thurst(10.0f);
				}
				break;

			case Keys.NumPad1:
				{
					vehicle?.Delete();
				}
				break;

			case Keys.NumPad2:
				{
					vehicle?.Thurst(10.0f);
				}
				break;

			default:
				break;
		}
	}
}
