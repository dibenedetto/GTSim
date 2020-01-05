using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace GTASim
{
	public class GTEnvironment : GTASim.Environment
	{
		private float          framesPerSecond     = 0.0f;
		private int            recordedFramesCount = 0;
		private int            maxStepsPerEpisode  = 0;
		private float          waitTime            = 0.0f;
		private int            width               = 0;
		private int            height              = 0;
		private TimeController controller          = null;
		private List<float[]>  frames              = null;

		public GTEnvironment(int maxStepsPerEpisode, int recordedFramesCount, float framesPerSecond)
			: base(maxStepsPerEpisode)
		{
			this.recordedFramesCount = recordedFramesCount;
			this.framesPerSecond     = framesPerSecond;
			this.waitTime            = 1.0f / framesPerSecond;
			this.controller          = new TimeController();
			this.frames              = new List<float[]>();

			ExternalSceneMaskSize(out width, out height);

			// states
			/////////////////////////////////////////////////////
			if (recordedFramesCount > 0)
			{
				AddStateDescriptor(new State.Descriptor
				{
					name  = "frames",
					type  = State.Descriptor.Type.Continuous,
					shape = new int[]{ recordedFramesCount * 3, height, width },
					min   = -1.0f,
					max   = +1.0f
				});
			}
			/////////////////////////////////////////////////////
		}

		public int RecordedFramesCount
		{
			get { return recordedFramesCount; }
		}

		public float FramesPerSecond
		{
			get { return framesPerSecond; }
		}

		protected override Result DoReset()
		{
			controller.Resume();
			InitializeEpisode ();
			controller.Pause();
			InitializeFrames  ();
			return GetResult();
		}

		protected override Result DoStep(Action action)
		{
			PerformAction (action);
			UpdateFrames  ();
			return GetResult();
		}

		private void InitializeFrames()
		{
			if (recordedFramesCount <= 0) return;
			frames.Clear();
			AcquireFrames(recordedFramesCount);
		}

		private void UpdateFrames()
		{
			if (recordedFramesCount <= 0) return;
			frames.RemoveAt(0);
			AcquireFrames(1);
		}

		protected State.Value GetFrames()
		{
			if (recordedFramesCount <= 0) return null;

			int      frameSize = 3 * height * width;
			float [] values    = new float [recordedFramesCount * frameSize];
			int      offset    = 0;
			for (int i=0; i<recordedFramesCount; ++i)
			{
				Array.Copy(frames[i], 0, values, offset, frameSize);
				offset += frameSize;
			}

			return new State.Value
			{
				value = values
			};
		}

		private void AcquireFrames(int framesCount)
		{
			int wh = width * height;

			for (int i=0; i<framesCount; ++i)
			{
				controller.Run(waitTime);

				var pixels = new byte [wh * 4];
				var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

				BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
				ExternalGetColorBuffer(data.Scan0);
				Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
				bitmap.UnlockBits(data);

				var image = new float [wh * 3];
				for (int ii=0, j=0, kR=0*wh, kG=1*wh, kB=2*wh; ii<wh; i+=1, j+=4, kR+=1, kG+=1, kB+=1)
				{
					image[kR] = ((float)(pixels[j+0])) / 255.0f * 2.0f - 1.0f;
					image[kG] = ((float)(pixels[j+1])) / 255.0f * 2.0f - 1.0f;
					image[kB] = ((float)(pixels[j+2])) / 255.0f * 2.0f - 1.0f;
				}

				frames.Add(image);
			}
		}

		protected virtual float GetReward()
		{
			return 0.0f;
		}

		protected override State GetNextState()
		{
			return new State
			{
				values = new State.Value[1]
				{
					GetFrames()
				}
			};
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
				reward           = GetReward             (),
				nextState        = GetNextState          (),
				availableActions = GetActionAvailability (),
				terminated       = IsEpisodeTerminated   (),
				aborted          = IsEpisodeAborted      ()
			};
		}

		private const string GTAVISIONNATIVE_ASI = "C:\\Program Files\\Rockstar Games\\Grand Theft Auto V\\GTAVisionNative.asi";

		[DllImport(GTAVISIONNATIVE_ASI, EntryPoint = "SceneMaskSize", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern int ExternalSceneMaskSize(out Int32 width, out Int32 height);

		[DllImport(GTAVISIONNATIVE_ASI, EntryPoint = "export_get_color_buffer", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern int ExternalGetColorBuffer(IntPtr buffer);
	}
}
