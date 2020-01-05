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
			this.maxStepsPerEpisode = maxStepsPerEpisode;
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
					Normalize(stateDescriptors[i], result.nextState.values[i]);
				}

				for (int i=0; i<actionDescriptors.Count; ++i)
				{
					Normalize(actionDescriptors[i], result.availableActions[i]);
				}
			}

			return result;
		}

		public Result Step(Action action)
		{
			if (!resetDone || ((result != null) && (result.terminated || result.aborted))) return null;
			
			++totalEpisodesSteps;
			++currentEpisodeSteps;

			var prevResult = result;
			if (action != null)
			{
				for (int i=0; i<actionDescriptors.Count; ++i)
				{
					Normalize(prevResult.availableActions[i], action.values[i]);
				}
			}

			result    = DoStep(action);
			resetDone = !result.terminated && !result.aborted;

			if (result != null)
			{
				for (int i=0; i<stateDescriptors.Count; ++i)
				{
					Normalize(stateDescriptors[i], result.nextState.values[i]);
				}

				for (int i=0; i<actionDescriptors.Count; ++i)
				{
					Normalize(actionDescriptors[i], result.availableActions[i]);
				}
			}

			return result;
		}

		protected void AddStateDescriptor(State.Descriptor descriptor)
		{
			stateDescriptors.Add(descriptor);
		}

		protected void AddActionDescriptor(Action.Descriptor descriptor)
		{
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
					min = actionDescriptors[i].min,
					max = actionDescriptors[i].max
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
			if ((value != null) && (descriptor.min < descriptor.max))
			{
				float scale = 2.0f / (descriptor.max - descriptor.min);
				float bias  = -1.0f;
				for (int i=0; i<value.value.Length; ++i)
				{
					var v = Clamp(value.value[i], descriptor.min, descriptor.max);
					value.value[i] = (v - descriptor.min) * scale + bias;
				}
			}
		}

		private void Normalize(Item.MinMax descriptor, Action.Availability availability)
		{
			if ((availability != null) && (descriptor.min < descriptor.max))
			{
				float scale = 2.0f / (descriptor.max - descriptor.min);
				float bias  = -1.0f;

				var vmin = Clamp(availability.min, descriptor.min, descriptor.max);
				availability.min = (vmin - descriptor.min) * scale + bias;
				
				var vmax = Clamp(availability.max, descriptor.min, descriptor.max);
				availability.max = (vmax - descriptor.min) * scale + bias;
			}
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
