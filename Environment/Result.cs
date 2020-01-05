namespace GTSim
{
	public class Result : Serializable
	{
		public float                  reward           = 0.0f;
		public State                  nextState        = null;
		public Action.Availability [] availableActions = null;
		public bool                   terminated       = false;
		public bool                   aborted          = false;

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
