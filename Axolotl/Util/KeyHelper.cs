using System;
using System.Security.Cryptography;
using Axolotl.Groups.State;
using Axolotl.Protocol;
using Axolotl.ECC;
using Axolotl.State;
using System.Collections.Generic;

namespace Axolotl.Util
{
	public class KeyHelper
	{
		public static IdentityKeyPair GenerateIdentityKeyPair() 
		{
			ECKeyPair   keyPair   = Curve.GenerateKeyPair();
			IdentityKey publicKey = new IdentityKey(keyPair.PublicKey);
			return new IdentityKeyPair(publicKey, keyPair.PrivateKey);
		}

		public static UInt32 GenerateRegistrationId(bool extendedRange) 
		{
			try {

				var rnd = new CryptoRandomProvider();
				if (extendedRange) return (UInt32)rnd.Next(Int32.MaxValue - 1) + 1;
				else               return (UInt32)rnd.Next(16380) + 1;

			} catch (Exception e) {
				throw new InvalidOperationException("Assertion error", e);
			}
		}

		public static int GetRandomSequence(int max) 
		{
			try {
				return new CryptoRandomProvider().Next(max);
			} catch (Exception e) {
				throw new InvalidOperationException("AssertionError", e);
			}
		}

		public static List<PreKeyRecord> GeneratePreKeys(int start, int count) 
		{
			var results = new List<PreKeyRecord>();

			start--;

			for (int i = 0; i < count; i++) {
				results.Add(new PreKeyRecord((UInt32)((start + i) % (Medium.MAX_VALUE-1)) + 1, Curve.GenerateKeyPair()));
			}

			return results;
		}

		public static PreKeyRecord GenerateLastResortPreKey() 
		{
			ECKeyPair keyPair = Curve.GenerateKeyPair();
			return new PreKeyRecord(Medium.MAX_VALUE, keyPair);
		}

		
		public static SignedPreKeyRecord GenerateSignedPreKey(IdentityKeyPair identityKeyPair, UInt32 signedPreKeyId)
		{
			var keyPair   = Curve.GenerateKeyPair();
			var signature = Curve.CalculateSignature(identityKeyPair.PrivateKey, keyPair.PublicKey.Serialize());

			var Jan1st1970 = new DateTime
				(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			return new SignedPreKeyRecord(signedPreKeyId, (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds, keyPair, signature);
		}

		public static ECKeyPair GenerateSenderSigningKey() {
			return Curve.GenerateKeyPair();
		}

		public static byte[] GenerateSenderKey() {
			try {
				byte[] key = new byte[32];
				new CryptoRandomProvider().GetBytes(key);

				return key;
			} catch (Exception e) {
				throw new InvalidOperationException("Assertion error", e);
			}
		}

		public static int GenerateSenderKeyId() {
			try {
				return new CryptoRandomProvider().Next(int.MaxValue);
			} catch (Exception e) {
				throw new InvalidOperationException("Assertion error", e);
			}
		}
	}
}

