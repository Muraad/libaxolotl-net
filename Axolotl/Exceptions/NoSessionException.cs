using System;

namespace Axolotl
{
	public class NoSessionException : Exception
	{
		public NoSessionException (string s) : base(s)
		{
		}

		public NoSessionException (string s, Exception e) : base(s, e)
		{
		}

		public NoSessionException (Exception e) : base(string.Empty, e)
		{
		}
	}
}

