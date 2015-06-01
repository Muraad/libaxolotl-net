using System;
using Sodium;

namespace Axolotl.ECC
{
	public class Curve
	{
		public const byte DJB_TYPE = 0x05;

		public static ECKeyPair GenerateKeyPair()
		{
			var keyPair = Sodium.PublicKeyAuth.GenerateKeyPair ();
			var sKey = Sodium.PublicKeyAuth.ConvertEd25519SecretKeyToCurve25519SecretKey (keyPair.PrivateKey);
			var pKey = Sodium.PublicKeyAuth.ConvertEd25519PublicKeyToCurve25519PublicKey (keyPair.PublicKey);


			return new ECKeyPair (new DjbECPrivateKey(sKey), new DjbECPublicKey(pKey));
		}

		public static ECPublicKey DecodePoint (byte[] bytes, int offset)
		{
			int type = bytes [offset] & 0xFF;

			switch (type) {
				case Curve.DJB_TYPE:
					byte[] keyBytes = new byte[32];
					Array.Copy (bytes, keyBytes, bytes.Length);
					return new DjbECPublicKey (keyBytes);
				default:
					throw new Exception ("Bad key type");
			}
		}

		public static ECPrivateKey DecodePrivatePoint(byte[] bytes)
		{
			return new DjbECPrivateKey (bytes);
		}

		public static byte[] CalculateAgreement(ECPublicKey publicKey, ECPrivateKey privateKey)
		{
			if (publicKey.GetKeyType () != privateKey.GetKeyType ()) {
				throw new Exception ("Public and private keys must be of the same type!");
			}

			if (publicKey.GetKeyType () == DJB_TYPE) {
				var shared = Sodium.ScalarMult.Mult (privateKey.Serialize (), publicKey.Serialize ());
				return shared;
			} else {
				throw new Exception ("Unknown type");
			}
		}

		public static bool VerifySignature(ECPublicKey signingKey, byte[] message, byte[] signature)
		{
			if (signingKey.GetKeyType () == DJB_TYPE) {
				return Sodium.OneTimeAuth.Verify (message, signature, signingKey.Serialize ());
			} else {
				throw new Exception ("Unknown type");
			}
		}

		public static byte[] CalculateSignature(ECPrivateKey privateKey, byte[] message)
		{
			if (privateKey.GetKeyType () == DJB_TYPE) {
				var sign = Sodium.SecretKeyAuth.SignHmacSha256 (message, privateKey.Serialize ());
				return sign;
			} else {
				throw new Exception ("Unknown type");
			}
		}
	}
}

