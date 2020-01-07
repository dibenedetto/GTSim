using System.Collections.Generic;

namespace GTSim
{
	public class Serializable
	{
		protected virtual bool FromJson(Dictionary<string, object> dict)
		{
			return false;
		}

		protected virtual bool ToJson(Dictionary<string, object> dict)
		{
			return false;
		}

		public bool FromString(string str)
		{

			return false;
		}

		public string ToString()
		{
			return null;
		}
	}
}
