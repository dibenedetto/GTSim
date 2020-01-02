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

		public Traffic()
		{
			;
		}

		public void Clear()
		{
			Function.Call(Hash.CLEAR_AREA_OF_VEHICLES, 0.0f, 0.0f, 0.0f, 1000000.0f, false, false, false, false, false);

			foreach (var vehicle in trafficVehicles)
			{
				vehicle.Delete();
			}
			trafficVehicles.Clear();

			drivingVehicle?.Delete();
			drivingVehicle = null;
		}

		public void Update()
		{
			foreach (var vehicle in trafficVehicles)
			{
				vehicle.Update();
			}

			drivingVehicle?.Update();
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

		public void ResetTrafficCamera()
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

			if (positions.Count > 0)
			{
				const float fmax = 999999999999.9f;
				Vector3 bmin = new Vector3(+fmax, +fmax, +fmax);
				Vector3 bmax = new Vector3(-fmax, -fmax, -fmax);

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

			if (cameraFOV < 0.1f)
			{
				ResetTrafficCamera();
			}

			var camera = World.CreateCamera(cameraPosition, cameraRotation, cameraFOV);
			camera.IsActive = true;

			World.RenderingCamera = camera;
		}
	}
}
