using System;

using GTA;
using GTA.Math;
using GTA.Native;

namespace GTSim
{
	public class DrivingVehicle : TrafficVehicle
	{
		Camera camera;

		public DrivingVehicle(string name, Model model, Vector3 position, float heading, bool onStreet, float maxSpeedMS)
			: base(name, model, position, heading, onStreet, maxSpeedMS)
		{
			SetupDriver();
		}

		public DrivingVehicle(Status status)
			: base(status)
		{
			SetupDriver();
		}

		private void SetupDriver()
		{
			if (driver != null)
			{
				driver.Delete();
				driver = null;
			}
			driver = Game.Player.Character;
			driver.SetIntoVehicle(vehicle, VehicleSeat.Driver);
		}

		protected override void DoUpdate(TimeSpan now)
		{
			if (camera != null)
			{
				camera.Rotation = vehicle.Rotation;
			}
		}

		public void ActivateCamera()
		{
			World.DestroyAllCameras();

			var offset = ((vehicle.Model == VehicleHash.Packer) ? (new Vector3(0.0f, 2.35f, 1.7f)) : (new Vector3(0.0f, 0.5f, 0.8f)));
			camera = World.CreateCamera(Vector3.Zero, Vector3.Zero, 60.0f);
			camera.AttachTo(vehicle, offset);

			camera.Rotation = vehicle.Rotation;
			Function.Call(Hash.SET_CAM_INHERIT_ROLL_VEHICLE, camera.Handle, (int)1);
			camera.IsActive = true;

			World.RenderingCamera = camera;
		}

		public void Noop(float value)
		{
			;
		}

		public void Thurst(float value)
		{
			//Function.Call(Hash._SET_CONTROL_NORMAL, 27, GTA.Control.VehicleAccelerate, value);
			Speed = value;
		}

		public void Brake(float value)
		{
			Function.Call(Hash._SET_CONTROL_NORMAL, 27, GTA.Control.VehicleBrake, value);
		}

		public void Steer(float value)
		{
			Function.Call(Hash._SET_CONTROL_NORMAL, 27, GTA.Control.VehicleMoveLeftRight, value);
		}

		public bool CameraIsActive
		{
			get { return camera.IsActive;  }
			set { camera.IsActive = value; }
		}
	}
}
