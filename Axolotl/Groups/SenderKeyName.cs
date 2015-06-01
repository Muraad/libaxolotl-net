using System;
using Axolotl;

namespace Axolotl.Groups
{
	// Completed
	public class SenderKeyName
	{
		public string GroupID { get; private set; }
		public AxolotlAddress Sender { get; private set; }

		public SenderKeyName (string groupId, AxolotlAddress sender)
		{
			GroupID = groupId;
			Sender = sender;
		}

		public string Serialize()
		{
			return string.Format ("{0}::{1}::{2}", GroupID, Sender.Name, Sender.DeviceID.ToString ());
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (obj.GetType () != typeof(SenderKeyName))
				return false;

			SenderKeyName that = (SenderKeyName)obj;

			return GroupID.Equals (that.GroupID) && Sender.Equals (that.Sender);
		}

		public override int GetHashCode ()
		{
			return GroupID.GetHashCode () ^ Sender.GetHashCode ();
		}
	}
}

