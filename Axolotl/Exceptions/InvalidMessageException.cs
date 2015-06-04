using System;

namespace Axolotl
{
	public class InvalidMessageException : Exception
	{
		public InvalidMessageException (string s) : base(s)
		{
		}

		public InvalidMessageException (Exception e): base(string.Empty, e)
		{
		}
	}
}

