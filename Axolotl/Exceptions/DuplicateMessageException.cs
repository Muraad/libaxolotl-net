using System;

namespace Axolotl
{
	public class DuplicateMessageException : Exception
	{
		public DuplicateMessageException (string s) : base(s)
		{
		}
	}
}

