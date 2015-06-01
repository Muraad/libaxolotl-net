using System;
using System.Collections.Generic;

namespace Axolotl.Groups.State
{
	public class SenderKeyRecord
	{
		private LinkedList<SenderKeyState> senderKeyStates = new LinkedList<SenderKeyState>();

		public SenderKeyRecord () {}

		public SenderKeyRecord (byte[] serialized)
		{
			//TODO
			//SenderKeyRecordStructure senderKeyRecordStructure = new SenderKeyRecordStructure ();
		}
	}
}

