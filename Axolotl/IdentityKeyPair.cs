using System;
using System.IO;
using Axolotl.ECC;
using Axolotl.State;
using ProtoBuf;

namespace Axolotl
{
	public class IdentityKeyPair
	{
		// Complete

		public IdentityKey PublicKey { get; private set; }

		public ECPrivateKey PrivateKey { get; private set; }

		public IdentityKeyPair(IdentityKey publicKey, ECPrivateKey privateKey)
		{
			PublicKey = publicKey;
			PrivateKey = privateKey;
		}

		public IdentityKeyPair(byte[] serialized)
		{
			//TODO: check for errors

			using(var stream = new MemoryStream())
			{
				stream.Write(serialized, 0, serialized.Length);
				var deserialized = Serializer.Deserialize<IdentityKeyPairStructure>(stream);

				PublicKey = new IdentityKey(deserialized.publicKey, 0);
				PrivateKey = Curve.DecodePrivatePoint(deserialized.privateKey);
			}
		}

		public byte[] Serialize()
		{
			var idkp = new IdentityKeyPairStructure {
				publicKey = PublicKey.Serialize(),
				privateKey = PrivateKey.Serialize()
			};

			using(var stream = new MemoryStream())
			{
				Serializer.Serialize<IdentityKeyPairStructure>(stream, idkp);
				return stream.ToArray ();
			}
		}
	}
}

