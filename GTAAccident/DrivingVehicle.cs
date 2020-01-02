using GTA;
using GTA.Math;
using GTA.Native;

namespace GTASim
{
	public class DrivingVehicle : TrafficVehicle
	{
		Camera camera;

		public static DrivingVehicle Create(string name, Model model, Vector3 position, float heading, bool onStreet, float maxSpeedMS, float speedMS, float steeringAngle)
		{
			var res = new DrivingVehicle(name, model, position, heading, onStreet, maxSpeedMS, speedMS, steeringAngle);
			res.SetupDriver();
			return res;
		}

		public static DrivingVehicle Create(Status status)
		{
			var res = new DrivingVehicle(status);
			res.SetupDriver();
			return res;
		}

		protected DrivingVehicle(string name, Model model, Vector3 position, float heading, bool onStreet, float maxSpeedMS, float speedMS, float steeringAngle)
			: base(name, model, position, heading, onStreet, maxSpeedMS, speedMS, steeringAngle)
		{
			;
		}

		protected DrivingVehicle(Status status)
			: base(status)
		{
			;
		}

		protected override void SetupDriver()
		{
			Game.Player.Character.SetIntoVehicle(vehicle, VehicleSeat.Driver);
		}

		protected override void DoUpdate()
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
			Function.Call(Hash._SET_CONTROL_NORMAL, 27, GTA.Control.VehicleAccelerate, value);
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
