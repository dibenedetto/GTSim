using System;
using System.Collections.Generic;
using System.IO;

using GTA;
using GTA.Math;
using GTA.Native;

namespace GTSim
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
			public float  maxSpeed      = 0.0f;
			public float  speed         = 0.0f;
			public float  steeringAngle = 0.0f;

			public Status ShallowCopy()
			{
				return (Status)this.MemberwiseClone();
			}
		}

		public class Keyframe
		{
			public TimeSpan offset        = TimeSpan.Zero;
			public float    speed         = -2.0f;
			public float    steeringAngle = -2.0f;

			public Keyframe ShallowCopy()
			{
				return (Keyframe)this.MemberwiseClone();
			}
		}

		protected Status  status        = null;
		protected Vehicle vehicle       = null;
		protected Ped     driver        = null;
		protected float   speed         = 0.0f;
		protected float   steeringAngle = 0.0f;

		protected float   speedBeforeDamage = 0.0f;
		protected float   steerBeforeDamage = 0.0f;

		protected List<Keyframe> timeline      = null;
		protected List<Keyframe> timelineFixed = null;
		protected TimeSpan       timelineStart = TimeSpan.Zero;
		protected int            timelineIndex = -1;
		protected double         timeFactor    = 1.0;

		public TrafficVehicle(string name, Model model, Vector3 position, float heading, bool onStreet, float maxSpeed)
		{
			var adjustedMaxSpeed     = Constants.MAX_SPEED_FRICTION_FACTOR * maxSpeed;
			var adjustedCurrentSpeed = 0.0f;
			var steeringAngle        = 0.0f;

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
				maxSpeed      = adjustedMaxSpeed,
				speed         = adjustedCurrentSpeed,
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
			Function.Call(Hash._SET_VEHICLE_MAX_SPEED, vehicle.Handle, status.maxSpeed);
			Speed         = status.speed;
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

			timeline = new List<Keyframe>();
		}

		public void Delete()
		{
			if (driver != Game.Player.Character)
			{
				driver.Delete();
			}
			vehicle.Delete();
		}

		void FixTimeline()
		{
			TimeSpan current = TimeSpan.Zero;
			timelineFixed = new List<Keyframe>();
			for (int i=0; i<timeline.Count; ++i)
			{
				var key = timeline[i];
				if (key.offset.TotalSeconds < 0.0) continue;
				var fixedKey = key;
				fixedKey.offset += current;
				current = fixedKey.offset;
				timelineFixed.Add(fixedKey);
			}

			timeFactor = ((double)(Function.Call<int>(Hash.GET_MILLISECONDS_PER_GAME_MINUTE))) / (1000.0 * 60.0);
		}

		void Apply(Keyframe key)
		{
			if (key.speed         >   0.0f) Speed         = key.speed;
			if (key.steeringAngle >= -1.0f) SteeringAngle = key.steeringAngle;
		}

		protected virtual void DoUpdate(TimeSpan now)
		{
			bool applied = false;
			if (IsStarted && (timelineFixed.Count > 0) && (timelineIndex < timelineFixed.Count))
			{
				var elapsed = (now - timelineStart).TotalSeconds * timeFactor;
				while (timelineIndex < timelineFixed.Count)
				{
					if (timelineFixed[timelineIndex].offset.TotalSeconds > elapsed) break;
					Apply(timelineFixed[timelineIndex]);
					applied = true;
					++timelineIndex;
				}
			}

			if (!applied)
			{
				Speed         = speed;
				SteeringAngle = steeringAngle;
			}
		}

		public void Update(TimeSpan now)
		{
			if (!IsDamaged)
			{
				speedBeforeDamage = Speed;
				steerBeforeDamage = SteeringAngle;
			}

			DoUpdate(now);
		}

		public string Log()
		{
			return null;
		}

		public string Name
		{
			get { return status.name; }
		}

		public Vehicle Vehicle
		{
			get { return vehicle; }
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

		public float MaxSpeed
		{
			get { return status.maxSpeed; }
		}

		public float Speed
		{
			get { return vehicle.Speed; }

			set
			{
				if (!IsDamaged)
				{
					speed                = value;
					vehicle.ForwardSpeed = value;
				}
			}
		}

		public float SteeringAngle
		{
			get { return vehicle.SteeringAngle; }

			set
			{
				if (!IsDamaged)
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
				const float engineMinHealt  = -4000.0f;
				const float engineMaxHealt  = +1000.0f;
				const float engineFactor    = 1.0f;
				const float engineScale     = engineFactor / (engineMaxHealt - engineMinHealt);

				const float bodyMinHealt    = +   0.0f;
				const float bodyMaxHealt    = +1000.0f;
				const float bodyFactor      = 1.0f;
				const float bodyScale       = bodyFactor / (bodyMaxHealt - bodyMinHealt);

				float       vehicleMinHealt = +0.0f;
				float       vehicleMaxHealt = vehicle.MaxHealthFloat;
				float       vehicleFactor   = 1.0f;
				float       vehicleScale    = vehicleFactor / (vehicleMaxHealt - vehicleMinHealt);

				float       globalFactor    = 1.0f / (engineFactor + bodyFactor + vehicleFactor);

				float engine = (vehicle.EngineHealth - engineMinHealt ) * engineScale ;
				float body   = (vehicle.BodyHealth   - bodyMinHealt   ) * bodyScale   ;
				float veh    = (vehicle.HealthFloat  - vehicleMinHealt) * vehicleScale;

				float healt  = globalFactor  * (engine + body + veh);

				return healt;
			}
		}

		public float Damage
		{
			get { return (1.0f - Healt); }
		}

		public bool IsDamaged
		{
			get { return (vehicle.IsDamaged || (Damage > 0.0f)); }
		}

		public List<Keyframe> Timeline
		{
			get { return timeline;  }
			set { timeline = value; }
		}

		public bool Start(Vector3 position, float heading, TimeSpan now)
		{
			Stop(now);
			FixTimeline();

			Position      = position;
			Heading       = heading;

			timelineStart = now;
			timelineIndex = 0;

			return true;
		}

		public bool Start(TimeSpan now, bool restoreState = true)
		{
			Vector3 position = ((restoreState) ? (new Vector3(status.positionX, status.positionY, status.positionZ)) : (vehicle.Position));
			float   heading  = ((restoreState) ? (status.heading                                                   ) : (vehicle.Heading ));
			return Start(vehicle.Position, vehicle.Heading, now);
		}

		public void Stop(TimeSpan now)
		{
			if (!IsStarted) return;
			timelineFixed   = null;
			timelineStart   = TimeSpan.Zero;
			timelineIndex   = -1;
			Speed           = 0.0f;
			SteeringAngle   = 0.0f;
		}

		public bool IsStarted
		{
			get { return (timelineIndex >= 0); }
		}
	}
}
