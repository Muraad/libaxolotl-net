using System;

namespace Axolotl
{
	public static class ArrayComparer
	{
		public static bool Compare(byte[] first, byte[] second)
		{
			if(first.Length != second.Length)
			{
				return false;
			}
			bool ret = true;
			for(int i = 0; i < first.Length; i++)
			{
				ret = ret & (first[i] == second[i]);
			}
			return ret;
		}
	}
}

