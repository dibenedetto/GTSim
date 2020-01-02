using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

using GTA;
using GTA.Math;
using GTA.Native;

using GTASim;

public class GTATest : Script
{
	Traffic traffic = null;

	public GTATest()
	{
		this.Tick    += OnTick   ;
		this.KeyDown += OnKeyDown;
		this.KeyUp   += OnKeyUp  ;

		traffic = new Traffic();
	}

	public void OnTick(object sender, EventArgs e)
	{
		traffic.Update();

		{
			int value = Game.GetControlValue(GTA.Control.LookLeftRight);
			File.AppendAllText("sbuthre.txt", "value : " + value + "\n");
		}

		if (false && traffic.DrivingVehicle != null)
		{
			{
				float value = 0.0f;
				if (Game.IsKeyPressed(Keys.Y))
				{
					value = 1.0f;
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
					Model   model         = VehicleHash.Futo;
					Vector3 position      = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 5.0f;
					float   heading       = Game.Player.Character.Heading + 0.0f;
					float   maxSpeedMS    = 150.0f * Constants.KMH_TO_MS;
					float   curSpeedMS    =   0.0f * Constants.KMH_TO_MS;
					float   steeringAngle = 0.0f;

					traffic.DrivingVehicle = DrivingVehicle.Create("stig", model, position, heading, true, maxSpeedMS, curSpeedMS, steeringAngle);
				}
				break;

			case Keys.NumPad1:
				{
					Vector3 position = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 10.0f;
					float   heading  = Game.Player.Character.Heading + 0.0f;

					/*
					{
						var pos = Game.Player.Character.Position;
						unsafe
						{
							Function.Call(Hash.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING, pos.X, pos.Y, pos.Z, &position, &heading, 0, 0, 0);
						}
					}
					*/

					Model   model      = VehicleHash.Futo;
					float   maxSpeedMS = 150.0f * Constants.KMH_TO_MS;
					float   curSpeedMS =   0.0f * Constants.KMH_TO_MS;

					var vehicle = TrafficVehicle.Create("stupre", model, position, heading, true, maxSpeedMS, curSpeedMS, 0.0f);
					traffic.TrafficVehicles.Add(vehicle);
				}
				break;

			case Keys.NumPad2:
				{
					traffic.Clear();
				}
				break;

			case Keys.NumPad4:
				{
					float speedMS = 70.0f * Constants.KMH_TO_MS;
					foreach (var vehicle in traffic.TrafficVehicles)
					{
						vehicle.Speed = speedMS;
					}
				}
				break;

			case Keys.NumPad5:
				{
					World.RenderingCamera = null;
				}
				break;

			case Keys.NumPad7:
				{
					float angle = +1.0f;
					foreach (var vehicle in traffic.TrafficVehicles)
					{
						vehicle.SteeringAngle = angle;
					}
				}
				break;

			case Keys.NumPad9:
				{
					traffic.ActivateGameCamera();
				}
				break;

			case Keys.NumPad6:
				{
					traffic.ActivateDrivingCamera();
				}
				break;

			case Keys.NumPad3:
				{
					traffic.ActivateTrafficCamera();
				}
				break;

			default:
				{
					;
				}
				break;
		}
	}
}
