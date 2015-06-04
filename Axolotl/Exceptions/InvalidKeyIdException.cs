using System;

namespace Axolotl
{
	public class InvalidKeyIdException : Exception
	{
		public InvalidKeyIdException (string s) : base(s)
		{
		}
	}
}

