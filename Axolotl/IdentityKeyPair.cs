using System;
using Axolotl.ECC;
using Axolotl.State;

namespace Axolotl
{
	public class IdentityKeyPair
	{
		public IdentityKey PublicKey { get; private set; }
		public ECPrivateKey PrivateKey { get; private set; }

		public IdentityKeyPair (IdentityKey publicKey, ECPrivateKey privateKey)
		{
			PublicKey = publicKey;
			PrivateKey = privateKey;
		}

		public IdentityKeyPair (byte[] serialized)
		{
			try {
				// TODO
			}
			catch {
				throw new Exception ();
			}
		}

		public byte[] Serialize()
		{
			// TODO
			//return IdentityKeyPairStructure.newBuilder()
			//	.setPublicKey(ByteString.copyFrom(publicKey.serialize()))
			//		.setPrivateKey(ByteString.copyFrom(privateKey.serialize()))
			//		.build().toByteArray();
			return new byte[3];
		}
	}
}

