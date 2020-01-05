namespace GTSim
{
	public class Item : Serializable
	{
		public class MinMax : Serializable
		{
			public float min = 0.0f;
			public float max = 0.0f;

			public override bool FromJsonString(string str)
			{
				return false;
			}

			public override string ToJsonString()
			{
				return null;
			}
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

			public override bool FromJsonString(string str)
			{
				return false;
			}

			public override string ToJsonString()
			{
				return null;
			}
		}

		public class Value : Serializable
		{
			public float [] value = null;

			public override bool FromJsonString(string str)
			{
				return false;
			}

			public override string ToJsonString()
			{
				return null;
			}
		}

		public Value [] values = null;

		public override bool FromJsonString(string str)
		{
			return false;
		}

		public override string ToJsonString()
		{
			return null;
		}
	}
}
