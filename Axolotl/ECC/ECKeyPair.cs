using System;
using Axolotl.ECC;

namespace Axolotl.ECC
{
	public class ECKeyPair
	{
		public ECPrivateKey PrivateKey { get; private set; }
		public ECPublicKey	PublicKey { get; private set; }

		public ECKeyPair (ECPrivateKey privateKey, ECPublicKey publicKey)
		{
			PrivateKey = privateKey;
			PublicKey = publicKey;
		}
	}
}