using System;
using Axolotl.ECC;
using System.IO;
using ProtoBuf;

namespace Axolotl.State
{
	// Almost complete
	public class SignedPreKeyRecord
	{
		private SignedPreKeyRecordStructure _structure;

		public UInt32 Id { 
			get { return _structure.Id.Value; }
		}

		public long Timestamp {
			get { return 0; }//return _structure.Timestamp; } UNDONE
		}

		public ECKeyPair keyPair {
			get {
				try {
					var publicKey = Curve.DecodePoint (_structure.PublicKey, 0);
					var privateKey = Curve.DecodePrivatePoint (_structure.PrivateKey);
					return new ECKeyPair (privateKey, publicKey);
				} catch (InvalidKeyException e) {
					throw new InvalidOperationException ("Assertion error", e);
				}
			}
		}

		public byte[] Signature {
			get { return _structure.Signature; }
		}

		public SignedPreKeyRecord (UInt32 id, long timestamp, ECKeyPair keyPair, byte[] signature)
		{
			_structure = new SignedPreKeyRecordStructure {
				Id = id,
				PublicKey = keyPair.PublicKey.Serialize(),
				PrivateKey = keyPair.PrivateKey.Serialize(),
				Signature =  signature,
				// Timestamp = timestamp //UNDONE
			};
		}

		public SignedPreKeyRecord (byte[] serialized)
		{
			using (var stream = new MemoryStream()) {
				// TODO: add check
				stream.Write (serialized, 0, serialized.Length);
				_structure = Serializer.Deserialize<SignedPreKeyRecordStructure> (stream);
			}
		}

		public byte[] Serialize()
		{
			using (var stream = new MemoryStream()) {
				Serializer.Serialize<SignedPreKeyRecordStructure> (stream, _structure);
				return stream.ToArray ();
			}
		}
	}
}

