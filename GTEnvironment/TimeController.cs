using System;

using GTA;

namespace GTSim
{
	public class TimeController
	{
		public static TimeSpan Now
		{
			get { return World.CurrentTimeOfDay; }
		}

		public TimeController(float timeScale = 1.0f)
		{
			Scale = timeScale;
		}

		public float Scale { get; set; }

		public void Reset()
		{
			Resume();
		}

		public void Pause()
		{
			Game.Pause(true);
		}

		public void Resume()
		{
			Game.Pause(false);
		}

		public void Run(float seconds)
		{
			Game.TimeScale = Scale;
			Resume();

			var t0 = Now;
			while ((Now - t0).TotalSeconds < seconds)
			{
				Script.Yield();
			}

			Pause();
			Game.TimeScale = 1.0f;
		}
	}
}
