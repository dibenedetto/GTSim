using System;
using System.Collections.Generic;

using GTA;
using GTA.Math;

namespace GTSim
{
	public class GTAccident : GTEnvironment
	{
		private float   actionValue = 0.0f;
		private Traffic traffic     = null;

		public GTAccident(float maxSecondsPerEpisode, int recorderFramesCount, float framesPerSecond)
			: base(maxSecondsPerEpisode, recorderFramesCount, framesPerSecond)
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
			traffic.InitializeRandomEpisode();
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

			var vehicle = traffic.DrivingVehicle.Vehicle;

			float damageValue = ((vehicle.MaxHealthFloat - vehicle.HealthFloat) / vehicle.MaxHealthFloat);
			float reward      = actionFactor * actionValue + damageFactor * damageValue;

			return reward;
		}
	}
}
