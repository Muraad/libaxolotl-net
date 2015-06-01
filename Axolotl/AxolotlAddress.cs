using System;

namespace Axolotl
{
	public class AxolotlAddress
	{
		public string Name { get; private set; }
		public int DeviceID { get; private set; }

		public AxolotlAddress (string name, int deviceId)
		{
			Name = name;
			DeviceID = deviceId;
		}

		public override string ToString ()
		{
			return string.Format ("{0}:{1}", Name, DeviceID);
		} 

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;

			if (obj.GetType () != typeof(AxolotlAddress))
				return false;
			AxolotlAddress that = (AxolotlAddress)obj;
			return this.Name.Equals (that.Name) && this.DeviceID == that.DeviceID;
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode () ^ DeviceID;
		}
	}
}

