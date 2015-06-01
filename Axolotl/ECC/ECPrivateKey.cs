using System;

namespace Axolotl.ECC
{
	public abstract class ECPrivateKey
	{
		public abstract byte[] Serialize();
		public abstract int GetKeyType();
	}
}

