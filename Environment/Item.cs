namespace GTSim
{
	public class Item
	{
		public class MinMax
		{
			public float min = 0.0f;
			public float max = 0.0f;
		}

		public class Descriptor : MinMax
		{
			public enum Type
			{
				None       = 0,
				Binary     = 1,
				Discrete   = 2,
				Continuous = 3
			};

			public string name  = null;
			public Type   type  = Type.None;
			public int [] shape = null;
		}

		public class Value
		{
			public float [] value = null;
		}

		public Value [] values = null;
	}
}
