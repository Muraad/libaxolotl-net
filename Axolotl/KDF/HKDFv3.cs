using System;

namespace Axolotl.KDF
{
	public class HKDFv3 : HKDF
	{
		protected override int GetIterationStartOffset ()
		{
			return 1;
		}
	}
}

