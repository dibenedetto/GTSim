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
			//int value = Game.GetControlValue(GTA.Control.LookLeftRight);
			//File.AppendAllText("sbuthre.txt", "value : " + value + "\n");
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
					Model   model      = VehicleHash.Futo;
					Vector3 position   = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 5.0f;
					float   heading    = Game.Player.Character.Heading + 0.0f;
					float   maxSpeedMS = 150.0f * Constants.KMH_TO_MS;

					traffic.DrivingVehicle = new DrivingVehicle("stig", model, position, heading, true, maxSpeedMS);

					/*
					{
						var pos = Game.Player.Character.Position;
						unsafe
						{
							Function.Call(Hash.GET_CLOSEST_VEHICLE_NODE_WITH_HEADING, pos.X, pos.Y, pos.Z, &position, &heading, 0, 0, 0);
						}
					}
					*/
				}
				break;

			case Keys.NumPad1:
				{
					Vector3 position   = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 10.0f;
					float   heading    = Game.Player.Character.Heading + 0.0f;
					Model   model      = VehicleHash.Futo;
					float   maxSpeedMS = 150.0f * Constants.KMH_TO_MS;

					var vehicle = new TrafficVehicle("stupre", model, position, heading, true, maxSpeedMS);

					vehicle.Timeline.keyframes.Add(null);

					vehicle.Timeline.keyframes.Add(new TrafficVehicle.DriveTimeline.Keyframe
					{
						offset        = TimeSpan.Zero,
						speedMS       = 70.0f * Constants.KMH_TO_MS,
						steeringAngle = 0.0f
					});

					vehicle.Timeline.keyframes.Add(new TrafficVehicle.DriveTimeline.Keyframe
					{
						offset        = new TimeSpan(0, 0, 2),
						speedMS       = 70.0f * Constants.KMH_TO_MS,
						steeringAngle = -1.0f
					});

					vehicle.Start();

					traffic.TrafficVehicles.Add(vehicle);
				}
				break;

			case Keys.NumPad2:
				{
					traffic.Clear();
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
