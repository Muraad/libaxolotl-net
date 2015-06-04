using System;

namespace Axolotl
{
	public class InvalidVersionException : Exception
	{
		public InvalidVersionException (string s) : base(s)
		{
		}
	}
}

