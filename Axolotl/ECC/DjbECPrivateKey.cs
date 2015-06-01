using System;

namespace Axolotl.ECC
{
	public class DjbECPrivateKey : ECPrivateKey
	{
		public byte[] PublicKey { get; private set; }

		public DjbECPrivateKey (byte[] privateKey) {
			PublicKey = privateKey;
		}

		public override byte[] Serialize ()
		{
			return PublicKey;
		}

		public override int GetKeyType ()
		{
			return Curve.DJB_TYPE;
		}
	}
}

