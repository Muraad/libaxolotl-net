using System;

namespace Axolotl.ECC
{
	public class Curve
	{
		public static byte DJB_TYPE = 0x05;

		public Curve ()
		{
		}

		public static ECPublicKey DecodePoint (byte[] bytes, int offset)
		{
			throw new NotImplementedException ();
		}
	}
}

