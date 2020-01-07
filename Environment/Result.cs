namespace GTSim
{
	public class Result : Serializable
	{
		public float                  Reward           { get; set; }
		public State                  NextState        { get; set; }
		public Action.Availability [] AvailableActions { get; set; }
		public bool                   Terminated       { get; set; }
		public bool                   Aborted          { get; set; }
	}
}
