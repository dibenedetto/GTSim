using System;
using System.Collections.Generic;

using GTA;
using GTA.Math;
using GTA.Native;

namespace GTSim
{
	public class Traffic
	{
		Vector3              cameraPosition  = Vector3.Zero;
		Vector3              cameraRotation  = Vector3.Zero;
		float                cameraFOV       = -1.0f;
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

			cameraFOV = -1.0f;
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
			Clear();

			//if (false)
			{
				Vector3 position   = Game.Player.Character.Position + Game.Player.Character.ForwardVector * 10.0f;
				float   heading    = Game.Player.Character.Heading + 0.0f;
				Model   model      = VehicleHash.Futo;
				float   maxSpeedMS = Constants.MAX_SPEED;

				var vehicle = new DrivingVehicle("pluto", model, position, heading, true, maxSpeedMS);
				DrivingVehicle = vehicle;
			}

			//if (false)
			{
				Vector3 position   = DrivingVehicle.Position + DrivingVehicle.Vehicle.ForwardVector * 30.0f;
				float   heading    = DrivingVehicle.Heading + 180.0f;
				Model   model      = VehicleHash.Futo;
				float   maxSpeedMS = Constants.MAX_SPEED;

				var vehicle = new TrafficVehicle("pippo", model, position, heading, false, maxSpeedMS);

				vehicle.Timeline.Add(new TrafficVehicle.Keyframe
				{
					offset        = TimeSpan.Zero,
					speed         = 70.0f * Constants.KMH_TO_MS,
					steeringAngle = 0.0f
				});

				vehicle.Timeline.Add(new TrafficVehicle.Keyframe
				{
					offset        = new TimeSpan(0, 0, 1),
					speed         = 70.0f * Constants.KMH_TO_MS,
					steeringAngle = -1.0f
				});

				TrafficVehicles.Add(vehicle);
			}

			return Start();
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
			if (cameraFOV <= 0.0f) return;
			World.DestroyAllCameras();
			var camera = World.CreateCamera(cameraPosition, cameraRotation, cameraFOV);
			camera.IsActive = true;
			World.RenderingCamera = camera;
		}

		public bool Start()
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

			ActivateGameCamera    ();
			//ActivateDrivingCamera ();
			//ActivateTrafficCamera ();

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

			ResetTrafficCamera    ();
			ActivateTrafficCamera ();
		}

		public float Damage
		{
			get { return ((DrivingVehicle != null) ? (DrivingVehicle.Damage) : (0.0f)); }
		}

		protected void ResetTrafficCamera()
		{
			List<Vector3> positions = new List<Vector3>();
			foreach (var vehicle in trafficVehicles)
			{
				Vector3 fmin, fmax;
				(fmin, fmax) = vehicle.Vehicle.Model.Dimensions;
				Vector3 radius = (fmax - fmin) / 2.0f;

				positions.Add(vehicle.Position - radius);
				positions.Add(vehicle.Position + radius);
			}
			if (drivingVehicle != null)
			{
				Vector3 fmin, fmax;
				(fmin, fmax) = drivingVehicle.Vehicle.Model.Dimensions;
				Vector3 radius = (fmax - fmin) / 2.0f;

				positions.Add(drivingVehicle.Position - radius);
				positions.Add(drivingVehicle.Position + radius);
			}

			const float fovY = 60.0f;
			Vector3 position = Vector3.Zero;

			if (false && positions.Count > 0)
			{
				Vector3 bmin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				Vector3 bmax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

				foreach (var p in positions)
				{
					bmin = Vector2.Minimize(bmin, p);
					bmax = Vector2.Maximize(bmax, p);
				}

				const float expansion = 1.20f;
				float sceneRadius = (expansion * (bmax - bmin).Length()) / 2.0f;

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
