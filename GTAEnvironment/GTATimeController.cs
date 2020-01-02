using System.Diagnostics;

using GTA;

namespace GTASim
{
	public class GTATimeController
	{
		private Stopwatch stopwatch = new Stopwatch();
		private float     timeScale = 1.0f;

		public GTATimeController()
		{
			;
		}

		public GTATimeController(float timeScale)
		{
			TimeScale = timeScale;
		}

		public float TimeScale
		{
			get { return timeScale; }
			set { timeScale = ((value >= 0.0f) ? value : 0.0f); }
		}

		public float Wait(float seconds)
		{
			long waitMs = (long)(seconds / timeScale * 1000.0f);
			Game.TimeScale = timeScale;
			stopwatch.Restart();
			while (stopwatch.ElapsedMilliseconds < waitMs)
			{
				Script.Yield();
			}
			Game.TimeScale = 0.0f;
			stopwatch.Stop();
			return (((float)(stopwatch.ElapsedMilliseconds)) * timeScale / 1000.0f);
		}
	}
}
