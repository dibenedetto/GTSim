using System;
using System.Collections.Generic;

namespace GTSim
{
	public class Environment
	{
		private List<State  .Descriptor> stateDescriptors    = null;
		private List<Action .Descriptor> actionDescriptors   = null;

		private Result                   result              = null;
		private bool                     resetDone           = false;

		private int                      maxStepsPerEpisode  = 0;
		private int                      episodesCount       = 0;
		private int                      totalEpisodesSteps  = 0;
		private int                      currentEpisodeSteps = 0;

		public Environment(int maxStepsPerEpisode)
		{
			stateDescriptors  = new List<State  .Descriptor>();
			actionDescriptors = new List<Action .Descriptor>();
			this.maxStepsPerEpisode = Math.Max(1, maxStepsPerEpisode);
		}

		public List<State.Descriptor> StateDescriptors
		{
			get { return stateDescriptors; }
		}

		public List<Action.Descriptor> ActionDescriptors
		{
			get { return actionDescriptors; }
		}

		public int MaxStepsPerEpisode
		{
			get { return maxStepsPerEpisode; }
		}

		public int EpisodesCount
		{
			get { return episodesCount; }
		}

		public int TotalEpisodesSteps
		{
			get { return totalEpisodesSteps; }
		}

		public int CurrentEpisodeSteps
		{
			get { return currentEpisodeSteps; }
		}

		public Result LastResult
		{
			get { return result; }
		}

		public void Restart()
		{
			result              = null;
			resetDone           = false;
			episodesCount       = 0;
			totalEpisodesSteps  = 0;
			currentEpisodeSteps = 0;

			DoRestart();
		}

		public Result Reset()
		{
			++episodesCount;
			currentEpisodeSteps = 0;

			var prevResult = result;

			resetDone = false;
			result    = DoReset();
			resetDone = (result != null);

			if (result != null)
			{
				for (int i=0; i<stateDescriptors.Count; ++i)
				{
					Normalize(stateDescriptors[i], result.NextState.Values[i]);
				}

				for (int i=0; i<actionDescriptors.Count; ++i)
				{
					Normalize(actionDescriptors[i], result.AvailableActions[i]);
				}
			}

			return result;
		}

		public Result Step(Action action)
		{
			if (!resetDone || ((result != null) && (result.Terminated || result.Aborted))) return null;
			
			++totalEpisodesSteps;
			++currentEpisodeSteps;

			var prevResult = result;
			if (action != null)
			{
				for (int i=0; i<actionDescriptors.Count; ++i)
				{
					Normalize(prevResult.AvailableActions[i], action.Values[i]);
				}
			}

			result    = DoStep(action);

			if (result != null)
			{
				resetDone = !result.Terminated && !result.Aborted;

				for (int i=0; i<stateDescriptors.Count; ++i)
				{
					Normalize(stateDescriptors[i], result.NextState.Values[i]);
				}

				for (int i=0; i<actionDescriptors.Count; ++i)
				{
					Normalize(actionDescriptors[i], result.AvailableActions[i]);
				}
			}

			return result;
		}

		protected void AddStateDescriptor(State.Descriptor descriptor)
		{
			if (descriptor.Min > descriptor.Max)
			{
				float temp = descriptor.Min;
				descriptor.Min = descriptor.Max;
				descriptor.Max = temp;
			}

			stateDescriptors.Add(descriptor);
		}

		protected void AddActionDescriptor(Action.Descriptor descriptor)
		{
			if (descriptor.Min > descriptor.Max)
			{
				float temp = descriptor.Min;
				descriptor.Min = descriptor.Max;
				descriptor.Max = temp;
			}

			actionDescriptors.Add(descriptor);
		}

		protected virtual State GetNextState()
		{
			return null;
		}

		protected virtual Action.Availability[] GetActionAvailability()
		{
			Action.Availability[] actions = new Action.Availability[actionDescriptors.Count];
			for (int i=0; i<actions.Length; ++i)
			{
				actions[i] = new Action.Availability
				{
					Min = actionDescriptors[i].Min,
					Max = actionDescriptors[i].Max
				};
			}
			return actions;
		}

		private float Clamp(float value, float min, float max)
		{
			if (value <= min) return min;
			if (value >= max) return max;
			return value;
		}

		private void Normalize(Item.MinMax descriptor, Item.Value value)
		{
			if ((value != null) && (value.Data != null) && (descriptor.Min <= descriptor.Max))
			{
				/*
				float scale = 2.0f / (descriptor.Max - descriptor.Min);
				float bias  = -1.0f;
				for (int i=0; i<value.Data.Length; ++i)
				{
					var v = Clamp(value.Data[i], descriptor.Min, descriptor.Max);
					value.Data[i] = (v - descriptor.Min) * scale + bias;
				}
				*/
				for (int i = 0; i < value.Data.Length; ++i)
				{
					value.Data[i] = Clamp(value.Data[i], descriptor.Min, descriptor.Max);
				}
			}
		}

		private void Normalize(Item.MinMax descriptor, Action.Availability availability)
		{
			if ((availability != null) && (descriptor.Min <= descriptor.Max))
			{
				/*
				float scale = 2.0f / (descriptor.Max - descriptor.Min);
				float bias  = -1.0f;

				var vmin = Clamp(availability.Min, descriptor.Min, descriptor.Max);
				availability.Min = (vmin - descriptor.Min) * scale + bias;
				
				var vmax = Clamp(availability.Max, descriptor.Min, descriptor.Max);
				availability.Max = (vmax - descriptor.Min) * scale + bias;
				*/
				if (availability.Min > availability.Max)
				{
					float temp = availability.Min;
					availability.Min = availability.Max;
					availability.Max = temp;
				}

				availability.Min = Clamp(availability.Min, descriptor.Min, descriptor.Max);
				availability.Max = Clamp(availability.Max, descriptor.Min, descriptor.Max);
			}
		}

		protected virtual void DoRestart()
		{
			;
		}

		protected virtual Result DoReset()
		{
			return null;
		}

		protected virtual Result DoStep(Action action)
		{
			return null;
		}
	}
}
