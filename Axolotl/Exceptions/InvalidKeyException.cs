using System;

namespace Axolotl
{
	public class InvalidKeyException : Exception
	{
		public InvalidKeyException (string s) : base(s)
		{
		}
	}
}

