using System;
using System.Collections.Generic;

using GTA;
using GTA.Math;
using GTA.Native;

namespace GTASim
{
	public class Traffic
	{
		Vector3              cameraPosition  = Vector3.Zero;
		Vector3              cameraRotation  = Vector3.Zero;
		float                cameraFOV       = 0.0f;
		DrivingVehicle       drivingVehicle  = null;
		List<TrafficVehicle> trafficVehicles = new List<TrafficVehicle>();
		bool                 started         = false;

		public Traffic()
		{
			;
		}

		public void Clear()
		{
			Stop();

			Function.Call(Hash.CLEAR_AREA_OF_VEHICLES, 0.0f, 0.0f, 0.0f, 1000000.0f, false, false, false, false, false);

			foreach (var vehicle in trafficVehicles)
			{
				vehicle.Delete();
			}
			trafficVehicles.Clear();

			drivingVehicle?.Delete();
			drivingVehicle = null;
		}

		public DrivingVehicle DrivingVehicle
		{
			get { return drivingVehicle; }
			set
			{
				drivingVehicle?.Delete();
				drivingVehicle = value;
			}
		}

		public List<TrafficVehicle> TrafficVehicles
		{
			get { return trafficVehicles; }
		}

		public bool InitializeRandomEpisode()
		{
			/*
				{
					id      : 123,
					version : 1,

					meta : {
						name        : "stune",
						description : "desc",
						author      : "name",
						date        : "date"
					}

					scenario : {
						vehicles : {
							driving : {
								name      : "Driver_0",
								model     : "vehicle_mesh",
								position  : [x, y, z],
								heading   : 90.0,
								max_speed : 170.0 / 3.6,
								speed     : 70.0 / 3.6
							},

							traffic : [
								{
									name      : "Traffic_0",
									model     : "vehicle_mesh",
									position  : [x, y, z],
									heading   : 90.0,
									max_speed : 170.0 / 3.6,
									speed     : 70.0 / 3.6,

									timeline  : [
										{
											offset   : 0.0,
											speed    : 100.0,
											steering : 0.0
										},
										{
											offset   : 0.0,
											speed    : 100.0,
											steering : 0.0
										}
									]
								},
							]
						}
					}
				}




				{
					driving : {
						name      : "Driver_0",
						model     : "vehicle_mesh",
						position  : [x, y, z],
						heading   : 90.0,
						max_speed : 170.0 / 3.6,
						speed     : 70.0 / 3.6
					},

					traffic : [
						{
							name      : "Traffic_0",
							model     : "vehicle_mesh",
							position  : [x, y, z],
							heading   : 90.0,
							max_speed : 170.0 / 3.6,
							speed     : 70.0 / 3.6,

							timeline  : [
								{
									offset   : 0.0,
									speed    : 100.0,
									steering : 0.0
								},
								{
									offset   : 1.3,
									speed    : 100.0,
									steering : 0.0
								}
							]
						},
					]
				}
			*/

			Clear();

			{
				Vector3 position   = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 10.0f;
				float   heading    = Game.Player.Character.Heading + 0.0f;
				Model   model      = VehicleHash.Futo;
				float   maxSpeedMS = 150.0f * Constants.KMH_TO_MS;

				var vehicle = new TrafficVehicle("stupre", model, position, heading, true, maxSpeedMS);

				vehicle.Timeline.Add(new TrafficVehicle.Keyframe
				{
					offset        = TimeSpan.Zero,
					speed         = 70.0f * Constants.KMH_TO_MS,
					steeringAngle = 0.0f
				});

				vehicle.Timeline.Add(new TrafficVehicle.Keyframe
				{
					offset        = new TimeSpan(0, 0, 2),
					speed         = 70.0f * Constants.KMH_TO_MS,
					steeringAngle = -1.0f
				});

				TrafficVehicles.Add(vehicle);
			}
			
			return true;
		}

		public void ActivateGameCamera()
		{
			World.DestroyAllCameras();
			World.RenderingCamera = null;
		}

		public void ActivateDrivingCamera()
		{
			if (DrivingVehicle == null) return;
			World.DestroyAllCameras();
			DrivingVehicle.ActivateCamera();
		}

		public void ActivateTrafficCamera()
		{
			World.DestroyAllCameras();
			var camera = World.CreateCamera(cameraPosition, cameraRotation, cameraFOV);
			camera.IsActive = true;
			World.RenderingCamera = camera;
		}

		public bool Start(bool restoreState = true)
		{
			Stop();
			started = true;

			var now = World.CurrentTimeOfDay;
			foreach (var vehicle in trafficVehicles)
			{
				vehicle.Start(now);
			}
			drivingVehicle?.Start(now);

			foreach (var vehicle in trafficVehicles)
			{
				vehicle.Update(now);
			}
			drivingVehicle?.Update(now);

			ResetTrafficCamera();

			return true;
		}

		public void Stop()
		{
			if (!IsStarted) return;
			started = false;

			var now = World.CurrentTimeOfDay;
			foreach (var vehicle in trafficVehicles)
			{
				vehicle.Stop(now);
			}
			drivingVehicle?.Stop(now);
		}

		public bool IsStarted
		{
			get { return started; }
		}

		public void Update()
		{
			var now = World.CurrentTimeOfDay;
			foreach (var vehicle in trafficVehicles)
			{
				vehicle.Update(now);
			}
			drivingVehicle?.Update(now);
		}

		protected void ResetTrafficCamera()
		{
			List<Vector3> positions = new List<Vector3>();
			foreach (var vehicle in trafficVehicles)
			{
				positions.Add(vehicle.Position);
			}
			if (drivingVehicle != null)
			{
				positions.Add(drivingVehicle.Position);
			}

			const float fovY = 60.0f;
			Vector3 position = Vector3.Zero;

			if (false && positions.Count > 0)
			{
				Vector3 bmin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				Vector3 bmax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

				foreach (var p in positions)
				{
					bmin = Vector3.Minimize(bmin, p);
					bmax = Vector3.Maximize(bmax, p);
				}

				const float expansion = 1.20f;
				var sceneRadius = (expansion * (bmax - bmin).Length()) / 2.0f;

				var angle  = ((double)fovY / 180.0 * Math.PI);
				var radius = (double)sceneRadius / Math.Cos(angle);
				var height = radius * Math.Sin(angle);

				position   = (bmax + bmin) / 2.0f;
				position.Z = (float)height;
			}
			else
			{
				position   = Game.Player.Character.Position;
				position.Z = 100.0f;
			}

			cameraPosition = position;
			cameraRotation = new Vector3(-90.0f, 0.0f, 0.0f);
			cameraFOV      = fovY;
		}
	}
}
