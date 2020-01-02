using System;

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

		protected Status  status        = null;
		protected Vehicle vehicle       = null;
		protected float   speedMS       = 0.0f;
		protected float   steeringAngle = 0.0f;

		protected float   speedBeforDamage = 0.0f;
		protected float   steerBeforDamage = 0.0f;

		public static TrafficVehicle Create(string name, Model model, Vector3 position, float heading, bool onStreet, float maxSpeedMS, float speedMS, float steeringAngle)
		{
			var res = new TrafficVehicle(name, model, position, heading, onStreet, maxSpeedMS, speedMS, steeringAngle);
			res.SetupDriver();
			return res;
		}

		public static TrafficVehicle Create(Status status)
		{
			var res = new TrafficVehicle(status);
			res.SetupDriver();
			return res;
		}

		protected TrafficVehicle(string name, Model model, Vector3 position, float heading, bool onStreet, float maxSpeedMS, float speedMS, float steeringAngle)
		{
			const float MAX_SPEED_FACTOR = 0.999f;

			var adjustedMaxSpeedMS     = Constants.MAX_SPEED_FRICTION_FACTOR * maxSpeedMS;
			var adjustedCurrentSpeedMS = Math.Min(Constants.MAX_SPEED_FRICTION_FACTOR * speedMS, adjustedMaxSpeedMS * MAX_SPEED_FACTOR);

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

		protected TrafficVehicle(Status status)
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

			speedBeforDamage = Speed;
			steerBeforDamage = SteeringAngle;
		}

		public void Delete()
		{
			vehicle.Delete();
		}

		protected virtual void SetupDriver()
		{
			vehicle.CreateRandomPedOnSeat(VehicleSeat.Driver);
		}

		protected virtual void DoUpdate()
		{
			if (!vehicle.IsDamaged)
			{
				vehicle.ForwardSpeed  = this.speedMS;
				//vehicle.SteeringAngle = steeringAngle;
			}
		}

		public void Update()
		{
			DoUpdate();

			if (!vehicle.IsDamaged)
			{
				speedBeforDamage = Speed;
				steerBeforDamage = SteeringAngle;
			}
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
			get { return vehicle.Position; }
		}

		public Quaternion Quaternion
		{
			get { return vehicle.Quaternion; }
		}

		public float Heading
		{
			get { return vehicle.Heading; }
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
				speedMS              = value;
				vehicle.ForwardSpeed = value;
			}
		}

		public float SteeringAngle
		{
			get { return vehicle.SteeringAngle; }

			set
			{
				steeringAngle         = value;
				vehicle.SteeringAngle = value;
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
	}
}
