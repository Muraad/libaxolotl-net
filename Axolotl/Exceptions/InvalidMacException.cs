using System;

namespace Axolotl
{
	public class InvalidMacException : Exception
	{
		public InvalidMacException (string s) : base(s)
		{
		}

		public InvalidMacException (Exception e) : base(string.Empty, e)
		{
		}
	}
}

