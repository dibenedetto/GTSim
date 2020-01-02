using System;
using System.Collections.Generic;

using GTA;
using GTA.Math;

namespace GTASim
{
	public class GTAAccident : GTAEnvironment
	{
		private const int MAX_STEPS_PER_EPISODE = 1000;

		private float   actionValue = 0.0f;
		private Vehicle vehicle     = null;

		public GTAAccident(int framesCount, float framesPerSecond, float timeScale)
			: base(framesCount, framesPerSecond, timeScale, MAX_STEPS_PER_EPISODE)
		{
			// states
			/////////////////////////////////////////////////////
			// no more state
			/////////////////////////////////////////////////////
			

			// actions
			/////////////////////////////////////////////////////
			AddActionDescriptor(new Action.Descriptor
			{
				name  = "thrust",
				type  = State.Descriptor.Type.Continuous,
				shape = new int[]{ 1 },
				min   = 0.0f,
				max   = 1.0f
			});

			AddActionDescriptor(new Action.Descriptor
			{
				name  = "brake",
				type  = State.Descriptor.Type.Continuous,
				shape = new int[]{ 1 },
				min   = 0.0f,
				max   = 1.0f
			});

			AddActionDescriptor(new Action.Descriptor
			{
				name  = "steering-angle",
				type  = State.Descriptor.Type.Continuous,
				shape = new int[]{ 1 },
				min   = -1.0f,
				max   = +1.0f
			});
			/////////////////////////////////////////////////////
		}

		protected override void InitializeEpisode()
		{
			// vehicle
			Model   model    = VehicleHash.Adder;
			Vector3 position = new Vector3(0.0f, 0.0f, 0.0f);
			float   heading  = 0.0f;
			vehicle = World.CreateVehicle(model, position, heading);
			vehicle.ForwardSpeed = 28.0f;

			// camera
			World.DestroyAllCameras();
			var camera = World.RenderingCamera;
			camera.AttachTo(vehicle, Vector3.Zero);

			// test scenarios
			/*
			 * ...
			 * 
			*/

		}

		protected override void PerformAction(Action action)
		{
			const float thurstFactor = +1.0f;
			const float brakeFactor  = +1.0f;
			const float steerFactor  = +1.0f;

			actionValue = 0.0f;

			{
				// thrust
				{
					float value = 0.0f;
					if (action.values[0] != null)
					{
						value = action.values[0].value[0];
						actionValue += thurstFactor;
					}
					Game.SetControlValueNormalized(Control.VehicleAccelerate, value);
				}

				// brake
				{
					float value = 0.0f;
					if (action.values[1] != null)
					{
						value = action.values[1].value[0];
						actionValue += brakeFactor;
					}
					Game.SetControlValueNormalized(Control.VehicleBrake, value);
				}

				// steering
				{
					float value = 0.0f;
					if (action.values[2] != null)
					{
						value = action.values[2].value[0];
						actionValue += steerFactor;
					}
					Game.SetControlValueNormalized(Control.VehicleMoveLeftRight, value);
				}
			}

			actionValue /= (thurstFactor + brakeFactor + steerFactor);
		}

		protected override float GetReward()
		{
			const float actionFactor = -0.1f;
			const float damageFactor = -1.0f;

			float damageValue = ((vehicle.MaxHealthFloat - vehicle.HealthFloat) / vehicle.MaxHealthFloat);
			float reward      = actionFactor * actionValue + damageFactor * damageValue;

			return reward;
		}
	}
}
