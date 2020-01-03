using System;
using System.Collections.Generic;

using GTA;
using GTA.Math;
using GTA.Native;

namespace GTASim
{
	public class TrafficVehicle
	{
		public class Status
		{
			public string name          = null;
			public int    model         = 0;
			public float  positionX     = 0.0f;
			public float  positionY     = 0.0f;
			public float  positionZ     = 0.0f;
			public float  heading       = 0.0f;
			public float  maxSpeedMS    = 0.0f;
			public float  speedMS       = 0.0f;
			public float  steeringAngle = 0.0f;

			public Status ShallowCopy()
			{
				return (Status)this.MemberwiseClone();
			}
		}

		public class DriveTimeline
		{
			public class Keyframe
			{
				public TimeSpan offset        = TimeSpan.Zero;
				public float    speedMS       = -2.0f;
				public float    steeringAngle = -2.0f;
			}

			public List<Keyframe> keyframes;
		}

		protected Status  status        = null;
		protected Vehicle vehicle       = null;
		protected Ped     driver        = null;
		protected float   speedMS       = 0.0f;
		protected float   steeringAngle = 0.0f;

		protected float   speedBeforeDamage = 0.0f;
		protected float   steerBeforeDamage = 0.0f;

		protected DriveTimeline timeline      = null;
		protected TimeSpan      timelineStart = TimeSpan.Zero;
		protected int           timelineIndex = -1;

		public TrafficVehicle(string name, Model model, Vector3 position, float heading, bool onStreet, float maxSpeedMS)
		{
			var adjustedMaxSpeedMS     = Constants.MAX_SPEED_FRICTION_FACTOR * maxSpeedMS;
			var adjustedCurrentSpeedMS = 0.0f;
			var steeringAngle          = 0.0f;

			vehicle = World.CreateVehicle(model, position, heading);
			vehicle.PlaceOnGround();

			if (onStreet)
			{
				vehicle.PlaceOnNextStreet();
				var width = vehicle.Model.Dimensions.frontTopRight.Z - vehicle.Model.Dimensions.rearBottomLeft.Z;
				vehicle.Position += vehicle.RightVector * width * 2.0f;
			}

			this.status = new Status
			{
				name          = name,
				model         = model.Hash,
				positionX     = vehicle.Position.X,
				positionY     = vehicle.Position.Y,
				positionZ     = vehicle.Position.Z,
				heading       = vehicle.Heading,
				maxSpeedMS    = adjustedMaxSpeedMS,
				speedMS       = adjustedCurrentSpeedMS,
				steeringAngle = steeringAngle
			};

			Initialize();
		}

		public TrafficVehicle(Status status)
		{
			this.status = status.ShallowCopy();

			vehicle = World.CreateVehicle(new Model(status.model), new Vector3(status.positionX, status.positionY, status.positionZ), status.heading);
			vehicle.PlaceOnGround();

			Initialize();
		}

		void Initialize()
		{
			Function.Call(Hash._SET_VEHICLE_MAX_SPEED, vehicle.Handle, status.maxSpeedMS);
			Speed         = status.speedMS;
			SteeringAngle = status.steeringAngle;

			speedBeforeDamage = Speed;
			steerBeforeDamage = SteeringAngle;

			driver = vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);

			/*
			driver.DrivingSpeed = 10.0f;
			driver.DrivingStyle = DrivingStyle.Rushed;
			driver.MaxDrivingSpeed = 10.0f;
			driver.VehicleDrivingFlags = VehicleDrivingFlags.
			*/

			timeline = new DriveTimeline();
		}

		public void Delete()
		{
			vehicle.Delete();
		}

		protected virtual void DoUpdate()
		{
			if (IsStarted)
			{
				if (timeline.keyframes.Count > 0)
				{
					var now     = World.CurrentTimeOfDay;
					var elapsed = now - timelineStart;

					if (timelineIndex < 0)
					{
						//while ()
						if (true)
						{

						}
					}
					else
					{
						while (true)
						{
							var key = timeline.keyframes[timelineIndex];
							var dif = now - key.offset;
						}
					}
				}
			}

			if (!vehicle.IsDamaged)
			{
				vehicle.ForwardSpeed  = this.speedMS;
				//vehicle.SteeringAngle = steeringAngle;
			}
		}

		public void Update()
		{
			if (!vehicle.IsDamaged)
			{
				speedBeforeDamage = Speed;
				steerBeforeDamage = SteeringAngle;
			}

			DoUpdate();
		}

		public string Log()
		{
			return null;
		}

		public string Name
		{
			get { return status.name; }
		}

		public Vector3 Position
		{
			get { return vehicle.Position;  }
			set { vehicle.Position = value; }
		}

		public Quaternion Quaternion
		{
			get { return vehicle.Quaternion; }
		}

		public float Heading
		{
			get { return vehicle.Heading;  }
			set { vehicle.Heading = value; }
		}

		public Vector3 Velocity
		{
			get { return vehicle.Velocity; }
		}

		public float Speed
		{
			get { return vehicle.Speed; }

			set
			{
				if (!vehicle.IsDamaged)
				{
					speedMS              = value;
					vehicle.ForwardSpeed = value;
				}
			}
		}

		public float SteeringAngle
		{
			get { return vehicle.SteeringAngle; }

			set
			{
				if (!vehicle.IsDamaged)
				{
					steeringAngle         = value;
					vehicle.SteeringAngle = value;
				}
			}
		}

		public float Healt
		{
			get
			{
				const float engineMaxHealt  = 1.0f;
				const float engineFactor    = 1.0f / engineMaxHealt;

				const float bodyMaxHealt    = 1.0f;
				const float bodyFactor      = 1.0f / bodyMaxHealt;
			
					  float vehicleMaxHealt = vehicle.MaxHealthFloat;
					  float vehicleFactor   = 1.0f / vehicle.MaxHealthFloat;

					  float globalFactor    = 1.0f / (engineFactor + bodyFactor + vehicleFactor);

				float engine = engineFactor  * vehicle.EngineHealth;
				float body   = bodyFactor    * vehicle.BodyHealth;
				float veh    = vehicleFactor * vehicle.HealthFloat;

				float healt  = globalFactor  * (engine + body + veh);

				return healt;
			}
		}

		public float Damage
		{
			get { return (1.0f - Healt); }
		}

		public DriveTimeline Timeline
		{
			get { return timeline;  }
			set { timeline = value; }
		}

		public bool Start(Vector3 position, float heading)
		{
			Stop();
			Position      = position;
			Heading       = heading;
			timelineStart = World.CurrentTimeOfDay;
			return true;
		}

		public bool Start()
		{
			return Start(vehicle.Position, vehicle.Heading);
		}

		public void Stop()
		{
			if (!IsStarted) return;
			timelineStart = TimeSpan.Zero;
			timelineIndex = -1;
			Speed         = 0.0f;
			SteeringAngle = 0.0f;
		}

		public bool IsStarted
		{
			get { return (timelineIndex < 0); }
		}
	}
}
