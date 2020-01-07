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

		public GTAccident(float maxSecondsPerEpisode = 10.0f, float framesPerSecond = 10.0f, int recorderFramesCount = 4)
			: base(maxSecondsPerEpisode, framesPerSecond, recorderFramesCount)
		{
			// states
			/////////////////////////////////////////////////////
			// no more state
			/////////////////////////////////////////////////////
			

			// actions
			/////////////////////////////////////////////////////
			AddActionDescriptor(new Action.Descriptor
			{
				Name  = "thrust",
				Type  = State.Descriptor.ItemType.Continuous,
				Shape = new int[]{ 1 },
				Min   = 0.0f,
				Max   = 1.0f
			});

			AddActionDescriptor(new Action.Descriptor
			{
				Name  = "brake",
				Type  = State.Descriptor.ItemType.Continuous,
				Shape = new int[]{ 1 },
				Min   = 0.0f,
				Max   = 1.0f
			});

			AddActionDescriptor(new Action.Descriptor
			{
				Name  = "steering-angle",
				Type  = State.Descriptor.ItemType.Continuous,
				Shape = new int[]{ 1 },
				Min   = -1.0f,
				Max   = +1.0f
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
					if (action.Values[0] != null)
					{
						value = action.Values[0].Data[0];
						actionValue += thurstFactor;
					}
					Game.SetControlValueNormalized(Control.VehicleAccelerate, value);
				}

				// brake
				{
					float value = 0.0f;
					if (action.Values[1] != null)
					{
						value = action.Values[1].Data[0];
						actionValue += brakeFactor;
					}
					Game.SetControlValueNormalized(Control.VehicleBrake, value);
				}

				// steering
				{
					float value = 0.0f;
					if (action.Values[2] != null)
					{
						value = action.Values[2].Data[0];
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
