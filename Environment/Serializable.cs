namespace GTSim
{
	public class Serializable
	{
		public virtual bool FromJsonString(string str)
		{
			return false;
		}

		public virtual string ToJsonString()
		{
			return null;
		}
	}
}
