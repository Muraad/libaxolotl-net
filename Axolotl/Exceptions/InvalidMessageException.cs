using System;
using System.Collections.Generic;

namespace Axolotl
{
	public class InvalidMessageException : Exception
	{
		public InvalidMessageException(string s)
			: base(s)
		{
		}

		public InvalidMessageException(Exception e)
			: base(string.Empty, e)
		{
		}

		public InvalidMessageException(string s, Exception e)
			: base(s, e)
		{
			
		}

		public InvalidMessageException(string s, List<Exception> exceptions)
			: base(s)
		{
		}
	}
}

