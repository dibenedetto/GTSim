using System;
using System.Collections.Generic;
using System.IO;

using GTA;
using GTA.Math;

namespace GTSim
{
	public class GTAccident : GTEnvironment
	{
		private float   actionValue = 0.0f;
		private Traffic traffic     = null;

		public GTAccident(float maxSecondsPerEpisode = 10.0f, float framesPerSecond = 10.0f, float timeScale = 1.0f, int recorderFramesCount = 4, int frameWidth = 320, int frameHeight = 240, Random rand = null)
			: base(maxSecondsPerEpisode, framesPerSecond, timeScale, recorderFramesCount, frameWidth, frameHeight)
		{
			// states
			/////////////////////////////////////////////////////
			// no more state
			/////////////////////////////////////////////////////
			

			// actions
			/////////////////////////////////////////////////////
			AddActionDescriptor(new Action.Descriptor
			{
				Name  = "speed-keep",
				Type  = State.Descriptor.ItemType.Continuous,
				Shape = new int[]{ 1 },
				Min   = 0.0f,
				Max   = 1.0f
			});

			AddActionDescriptor(new Action.Descriptor
			{
				Name  = "speed-accelerate",
				Type  = State.Descriptor.ItemType.Continuous,
				Shape = new int[]{ 1 },
				Min   = 0.0f,
				Max   = 1.0f
			});

			AddActionDescriptor(new Action.Descriptor
			{
				Name  = "speed-brake",
				Type  = State.Descriptor.ItemType.Continuous,
				Shape = new int[]{ 1 },
				Min   = 0.0f,
				Max   = 1.0f
			});

			AddActionDescriptor(new Action.Descriptor
			{
				Name  = "steer-center",
				Type  = State.Descriptor.ItemType.Continuous,
				Shape = new int[]{ 1 },
				Min   = 0.0f,
				Max   = 1.0f
			});

			AddActionDescriptor(new Action.Descriptor
			{
				Name  = "steer-left",
				Type  = State.Descriptor.ItemType.Continuous,
				Shape = new int[]{ 1 },
				Min   = 0.0f,
				Max   = 1.0f
			});

			AddActionDescriptor(new Action.Descriptor
			{
				Name  = "steer-right",
				Type  = State.Descriptor.ItemType.Continuous,
				Shape = new int[]{ 1 },
				Min   = 0.0f,
				Max   = 1.0f
			});
			/////////////////////////////////////////////////////

			traffic = new Traffic(rand);
		}

		protected override void DoRestart()
		{
			base.DoRestart();
			traffic.Clear();
			traffic.ActivateGameCamera();
		}

		protected override void InitializeEpisode()
		{
			traffic.InitializeRandomEpisode();
		}

		protected override void PerformAction(Action action)
		{
			const float speedKeepFactor       = +0.0f;
			const float speedAccelerateFactor = +1.0f;
			const float speedBrakeFactor      = +1.0f;
			const float steerCenterFactor     = +0.0f;
			const float steerLeftFactor       = +1.0f;
			const float steerRightFactor      = +1.0f;

			actionValue = 0.0f;

			{
				// keep
				if (action.Values[0] != null)
				{
					float value  = action.Values[0].Data[0];
					actionValue += speedKeepFactor;
					traffic.DrivingVehicle?.Keep();
				}

				// accelerate
				if (action.Values[1] != null)
				{
					float value  = action.Values[1].Data[0];// / ActionDescriptors[0].Max;
					actionValue += speedAccelerateFactor;
					traffic.DrivingVehicle?.Accelerate(1.0f);
				}

				// brake
				if (action.Values[2] != null)
				{
					float value  = action.Values[2].Data[0];
					actionValue += speedBrakeFactor;
					traffic.DrivingVehicle?.Brake(1.0f);
				}

				// center
				if (action.Values[3] != null)
				{
					float value  = action.Values[3].Data[0];
					actionValue += steerCenterFactor;
					traffic.DrivingVehicle?.Steer(0.0f);
				}

				// left
				if (action.Values[4] != null)
				{
					float value  = action.Values[4].Data[0];
					actionValue += steerLeftFactor;
					traffic.DrivingVehicle?.Steer(-1.0f);
				}

				// right
				if (action.Values[5] != null)
				{
					float value  = action.Values[5].Data[0];
					actionValue += steerRightFactor;
					traffic.DrivingVehicle?.Steer(+1.0f);
				}
			}

			//actionValue /= (speedKeepFactor + speedAccelerateFactor + speedBrakeFactor + steerCenterFactor + steerLeftFactor + steerRightFactor);

			traffic.Update();
		}

		protected override float GetReward()
		{
			const float actionFactor = -    1.0f;
			const float damageFactor = - 1000.0f;

			float damageValue = traffic.Damage;
			float reward      = actionFactor * actionValue + damageFactor * damageValue;

			return reward;
		}
	}
}
