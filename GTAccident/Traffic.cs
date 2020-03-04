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
		Random               rand            = new Random(12345);

		public Traffic()
		{
			;
		}

		public void Clear()
		{
			Stop();

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

		public void Seed(int n)
		{
			rand = new Random(n);
		}

		public bool InitializeRandomEpisode()
		{
			Clear();

			InitializeClearWorld();

			World.CurrentTimeOfDay = new TimeSpan(12, 0, 0);

			//if (false)
			{
				//Vector3 position   = Game.Player.Character.Position;
				//float   heading    = Game.Player.Character.Heading;
				//Vector3 position   = new Vector3(-1306.281f, -2875.058f, 13.42174f);
				//float   heading    = 58.0f;
				Vector3 position   = new Vector3(-1172.419f, -1346.944f, 2.0f);
				float   heading    = 115.0f;
				Model   model      = VehicleHash.Futo;
				bool    onStreet   = false;
				float   maxSpeedMS = Constants.MAX_SPEED;

				var vehicle = new DrivingVehicle("pluto", model, position, heading, onStreet, maxSpeedMS);
				DrivingVehicle = vehicle;
			}

			//if (false)
			{
				float   distanceMin = 20.0f;
				float   distanceMax = distanceMin + 30.0f;
				float   distance    = distanceMin + (distanceMax - distanceMin) * ((float)(rand.NextDouble()));
				float   maxDrift    = 2.0f;
				float   drift       = -maxDrift * ((float)(rand.NextDouble()));

				distance = 30.0f;
				drift    = 0.0f;

				Vector3 position   = DrivingVehicle.Position + DrivingVehicle.Vehicle.ForwardVector * distance + DrivingVehicle.Vehicle.RightVector * drift;
				float   heading    = DrivingVehicle.Heading + 180.0f;
				Model   model      = VehicleHash.Futo;
				bool    onStreet   = false;
				float   maxSpeedMS = Constants.MAX_SPEED;

				var vehicle = new TrafficVehicle("pippo", model, position, heading, onStreet, maxSpeedMS);

				vehicle.Timeline.Add(new TrafficVehicle.Keyframe
				{
					offset        = TimeSpan.Zero,
					speed         = 70.0f * Constants.KMH_TO_MS,
					steeringAngle = 0.0f
				});

				/*
				vehicle.Timeline.Add(new TrafficVehicle.Keyframe
				{
					offset        = new TimeSpan(0, 0, 1),
					speed         = 70.0f * Constants.KMH_TO_MS,
					steeringAngle = -1.0f
				});
				*/

				TrafficVehicles.Add(vehicle);
			}

			bool res = Start();

			{
				DrivingVehicle?.Thurst(70.0f * Constants.KMH_TO_MS);
			}

			return res;
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

			//ActivateGameCamera    ();
			ActivateDrivingCamera ();
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
			UpdateClearWorld();

			var now = World.CurrentTimeOfDay;
			foreach (var vehicle in trafficVehicles)
			{
				vehicle.Update(now);
			}
			drivingVehicle?.Update(now);

			//ResetTrafficCamera    ();
			//ActivateTrafficCamera ();
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

		public static void InitializeClearWorld()
		{
			Function.Call(Hash.ADD_SCENARIO_BLOCKING_AREA, -10000.0f, -10000.0f, -1000.0f, 10000.0f, 10000.0f, 1000.0f, 0, 1, 1, 1);
			Function.Call(Hash.SET_CREATE_RANDOM_COPS, false);
			Function.Call(Hash.SET_RANDOM_BOATS, false);
			Function.Call(Hash.SET_RANDOM_TRAINS, false);
			Function.Call(Hash.SET_GARBAGE_TRUCKS, false);
			Function.Call(Hash.DELETE_ALL_TRAINS);
			Function.Call(Hash.SET_PED_POPULATION_BUDGET, 0);
			Function.Call(Hash.SET_VEHICLE_POPULATION_BUDGET, 0);
			Function.Call(Hash.SET_ALL_LOW_PRIORITY_VEHICLE_GENERATORS_ACTIVE, false);
			Function.Call(Hash.SET_NUMBER_OF_PARKED_VEHICLES, 0);    //  -1, 0
			Function.Call((Hash)0xF796359A959DF65D, false);  //Display distant vehicles
			Function.Call(Hash.DISABLE_VEHICLE_DISTANTLIGHTS, true);

			Ped[] all_ped = World.GetAllPeds();
			foreach (Ped ped in all_ped)
			{
				ped.Delete();
			}

			Vehicle[] all_vecs = World.GetAllVehicles();
			foreach (Vehicle vehicle in all_vecs)
			{
				vehicle.Delete();
			}
		}

		public static void UpdateClearWorld()
		{
			Function.Call(Hash.SET_VEHICLE_POPULATION_BUDGET, 0);
			Function.Call(Hash.SET_PED_POPULATION_BUDGET, 0);
			Function.Call(Hash.SET_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
			Function.Call(Hash.SET_RANDOM_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
			Function.Call(Hash.SET_PARKED_VEHICLE_DENSITY_MULTIPLIER_THIS_FRAME, 0f);
			Function.Call(Hash.STOP_ALARM, "PRISON_ALARMS", true);
			Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "re_prison");
			Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "re_prisonerlift");
			Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "am_prison");
			Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "re_lossantosintl");
			Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "re_armybase");
			Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "golf_ai_foursome");
			Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "golf_ai_foursome_putting");
			Function.Call((Hash)0x2F9A292AD0A3BD89);
			Function.Call((Hash)0x5F3B7749C112D552);
		}
	}
}
