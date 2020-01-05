using System.Diagnostics;

using GTA;

namespace GTSim
{
	public class TimeController
	{
		private Stopwatch stopwatch = new Stopwatch();

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

		public float Run(float seconds)
		{
			long waitMs = (long)(seconds * 1000.0f);
			Resume();
			stopwatch.Restart();
			while (stopwatch.ElapsedMilliseconds < waitMs)
			{
				Script.Yield();
			}
			stopwatch.Stop();
			Pause();
			return (((float)(stopwatch.ElapsedMilliseconds)) / 1000.0f);
		}
	}
}
