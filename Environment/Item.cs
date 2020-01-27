namespace GTSim
{
	public class Item
	{
		public class MinMax
		{
			public float Min { get; set; }
			public float Max { get; set; }
		}

		public class Descriptor : MinMax
		{
			public enum ItemType
			{
				None       = 0,
				Binary     = 1,
				Discrete   = 2,
				Continuous = 3,
				Image      = 4
			};

			public string   Name  { get; set; }
			public ItemType Type  { get; set; }
			public int []   Shape { get; set; }
		}

		public class Value
		{
			public float  [] Data  { get; set; }
			public string [] Image { get; set; }
		}

		public Value [] Values { get; set; }
	}
}
