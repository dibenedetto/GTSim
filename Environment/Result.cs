namespace GTASim
{
	public class Result
	{
		public float                  reward           = 0.0f;
		public State                  nextState        = null;
		public Action.Availability [] availableActions = null;
		public bool                   terminated       = false;
		public bool                   aborted          = false;
	}
}
