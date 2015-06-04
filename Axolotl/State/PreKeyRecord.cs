using System;
using System.IO;
using Mono.Xml;
using ProtoBuf;
using Axolotl.ECC;

namespace Axolotl.State
{
	//Complete

	public class PreKeyRecord
	{
		private PreKeyRecordStructure structure;

		public UInt32? Id { 
			get { return structure.Id; } 
		}

		public ECKeyPair KeyPair {
			get { 
				try {
					ECPublicKey publicKey = Curve.DecodePoint(structure.PublicKey, 0);
					ECPrivateKey privateKey = Curve.DecodePrivatePoint(structure.PrivateKey);
					return new ECKeyPair(privateKey, publicKey);
				} catch (InvalidKeyException e) {
					throw new InvalidOperationException ("Asserion error", e);
				}
			}
		}

		public PreKeyRecord (int id, ECKeyPair keyPair)
		{
			structure = new PreKeyRecordStructure {
				Id = (UInt32)id,
				PublicKey = keyPair.PublicKey.Serialize(),
				PrivateKey = keyPair.PrivateKey.Serialize()
			};
		}

		public PreKeyRecord (byte[] serialized)
		{
			// TODO: Check if right
			using (var stream = new MemoryStream()) {
				stream.Write (serialized, 0, serialized.Length);
				structure = Serializer.Deserialize<PreKeyRecordStructure> (stream);
			}
		}

		public byte[] Serialize()
		{
			byte[] result;

			using (var stream = new MemoryStream()) {
				Serializer.Serialize<PreKeyRecordStructure> (stream, structure);
				result = stream.ToArray ();
			}

			return result;
		}
	}
}

