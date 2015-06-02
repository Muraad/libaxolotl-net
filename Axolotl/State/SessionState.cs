using System;

namespace Axolotl.State
{
	public class SessionState
	{
		public int Version { get; private set; }

		public byte[] AliceBaseKey { get; private set; }

		public SessionStructure Structure { get; private set; }

		public SessionState ()
		{
			throw new NotImplementedException ();
		}

		public SessionState (SessionStructure currentSession)
		{
			throw new NotImplementedException ();
		}
	}
}

