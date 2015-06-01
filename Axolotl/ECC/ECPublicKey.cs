using System;

namespace Axolotl.ECC
{
	public abstract class ECPublicKey
	{
		public static int KEY_SIZE = 33;

		public abstract byte[] Serialize();

		public abstract int CompareTo(ECPublicKey another);

		public abstract int GetType();
	}
}

