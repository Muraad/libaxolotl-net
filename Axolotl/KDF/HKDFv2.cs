using System;

namespace Axolotl.KDF
{
	public class HKDFv2 : HKDF
	{
		protected override int GetIterationStartOffset () 
		{
			return 0;
		}
	}
}

