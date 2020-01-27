using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace GTSim
{
	public class GTEnvironment : GTSim.Environment
	{
		private float          maxSecondsPerEpisode = 0.0f;
		private float          framesPerSecond      = 0.0f;
		private float          timeScale            = 0.0f;
		private int            recordedFramesCount  = 0;
		private float          waitTime             = 0.0f;
		private TimeController controller           = null;
		private List<string>   frames               = null;
		private int            width                = 0;
		private int            height               = 0;

		public GTEnvironment(float maxSecondsPerEpisode, float framesPerSecond, float timeScale, int recordedFramesCount)
			: base((int)(Math.Ceiling(maxSecondsPerEpisode * framesPerSecond)))
		{
			this.maxSecondsPerEpisode = maxSecondsPerEpisode;
			this.framesPerSecond      = framesPerSecond;
			this.timeScale            = timeScale;

			this.recordedFramesCount  = recordedFramesCount;
			this.waitTime             = 1.0f / framesPerSecond;
			this.controller           = new TimeController(timeScale);
			this.frames               = new List<string>();

			ExternalSceneMaskSize(out width, out height);
			//File.AppendAllText("sbuthre.txt", "size: " + width + " x " + height + "\n");
			width  = 1280;
			height = 720;

			// states
			/////////////////////////////////////////////////////
			if (recordedFramesCount > 0)
			{
				AddStateDescriptor(new State.Descriptor
				{
					Name  = "frames",
					Type  = State.Descriptor.ItemType.Image,
					Shape = new int[]{ recordedFramesCount, height, width },
					Min   = 0.0f,
					Max   = 64.0f
				});
			}
			/////////////////////////////////////////////////////
		}

		public float MaxSecondsPerEpisode
		{
			get { return maxSecondsPerEpisode; }
		}

		public float FramesPerSecond
		{
			get { return framesPerSecond; }
		}

		public float TimeScale
		{
			get { return timeScale; }
		}

		public int RecordedFramesCount
		{
			get { return recordedFramesCount; }
		}

		protected override void DoRestart()
		{
			controller.Resume();
		}

		protected override Result DoReset()
		{
			controller.Resume ();
			InitializeEpisode ();
			controller.Pause  ();
			InitializeFrames  ();
			return GetResult  ();
		}

		protected override Result DoStep(Action action)
		{
			PerformAction    (action);
			UpdateFrames     ();
			return GetResult ();
		}

		private void InitializeFrames()
		{
			if (recordedFramesCount <= 0)
			{
				//File.AppendAllText("sbuthre.txt", "ping\n");
				controller.Run(waitTime);
				return;
			}
			frames.Clear();
			AcquireFrames(recordedFramesCount);
		}

		private void UpdateFrames()
		{
			if (recordedFramesCount <= 0)
			{
				//File.AppendAllText("sbuthre.txt", "pong: " + waitTime  + "\n");
				controller.Run(waitTime);
				return;
			}
			frames.RemoveAt(0);
			AcquireFrames(1);
		}

		protected State.Value GetFrames()
		{
			if (recordedFramesCount <= 0) return null;

			string [] values    = new string [recordedFramesCount];
			for (int i=0; i<recordedFramesCount; ++i)
			{
				values[i] = frames[i];
			}

			return new State.Value
			{
				Image = values
			};
		}

		private void AcquireFrames(int framesCount)
		{
			int wh = width * height;

			for (int i=0; i<framesCount; ++i)
			{
				controller.Run(waitTime);

				var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

				BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
				ExternalGetColorBuffer(data.Scan0);
				bitmap.UnlockBits(data);
				string str = ImageUtility.ExportBase64(bitmap, ImageFormat.Jpeg, 50L);

				frames.Add(str);
			}
		}

		protected virtual float GetReward()
		{
			return 0.0f;
		}

		protected override State GetNextState()
		{
			var state = new State();
			if (recordedFramesCount > 0)
			{
				state.Values = new State.Value[1]
				{
					GetFrames()
				};
			};
			return state;
		}

		protected virtual bool IsEpisodeTerminated()
		{
			return (CurrentEpisodeSteps >= MaxStepsPerEpisode);
		}

		protected virtual bool IsEpisodeAborted()
		{
			return false;
		}

		protected virtual void InitializeEpisode()
		{
			;
		}

		protected virtual void PerformAction(Action action)
		{
			;
		}

		protected virtual Result GetResult()
		{
			return new Result
			{
				Reward           = GetReward             (),
				NextState        = GetNextState          (),
				AvailableActions = GetActionAvailability (),
				Terminated       = IsEpisodeTerminated   (),
				Aborted          = IsEpisodeAborted      ()
			};
		}

		private const string GTAVISIONNATIVE_ASI = "C:\\Program Files\\Rockstar Games\\Grand Theft Auto V\\GTAVisionNative.asi";

		[DllImport(GTAVISIONNATIVE_ASI, EntryPoint = "SceneMaskSize", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern int ExternalSceneMaskSize(out Int32 width, out Int32 height);

		[DllImport(GTAVISIONNATIVE_ASI, EntryPoint = "export_get_color_buffer", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern int ExternalGetColorBuffer(IntPtr buffer);
	}
}
