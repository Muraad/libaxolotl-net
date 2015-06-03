using System;

namespace Axolotl.KDF
{
	public abstract class HKDF
	{
		public int HASH_OUTPUT_SIZE = 32;

		public static HKDF CreateFor(UInt32 messageVersion)
		{
			switch (messageVersion) {
				case 2:
					return new HKDFv2 ();
				case 3:
					return new HKDFv3 ();
				default:
					throw new Exception ("Unknown version: " + messageVersion);
			}
		}

		public byte[] DeriveSecrets(byte[] inputKeyMaterial, byte[] info, int outputLength)
		{
			byte[] salt = new byte[HASH_OUTPUT_SIZE];
			return DeriveSecrets (inputKeyMaterial, salt, info, outputLength);
		}

		public byte[] DeriveSecrets(byte[] inputKeyMaterial, byte[] salt, byte[] info, int outputLength)
		{
			byte[] prk = Extract (salt, inputKeyMaterial);
			return Expand (prk, info, outputLength);
		}

		private byte[] Extract (byte[] salt, byte[] inputKeyMaterial)
		{
			try {
				// TODO
				//Mac mac = Mac.getInstance("HmacSHA256");
				//mac.init(new SecretKeySpec(salt, "HmacSHA256"));
				//return mac.doFinal(inputKeyMaterial);
			}
			catch {

			}

			return new byte[3];
		}

		private byte[] Expand (byte[] prk, byte[] info, int outputLength)
		{
			// TODO
			//int                   iterations     = (int) Math.ceil((double) outputSize / (double) HASH_OUTPUT_SIZE);
			//byte[]                mixin          = new byte[0];
			//ByteArrayOutputStream results        = new ByteArrayOutputStream();
			//int                   remainingBytes = outputSize;

			//for (int i= getIterationStartOffset();i<iterations + getIterationStartOffset();i++) {
			//	Mac mac = Mac.getInstance("HmacSHA256");
			//	mac.init(new SecretKeySpec(prk, "HmacSHA256"));
			//
			//	mac.update(mixin);
			//	if (info != null) {
			//		mac.update(info);
			//	}
			//	mac.update((byte)i);
			//
			//	byte[] stepResult = mac.doFinal();
			//	int    stepSize   = Math.min(remainingBytes, stepResult.length);
			//
			//	results.write(stepResult, 0, stepSize);
			//
			//	mixin          = stepResult;
			//	remainingBytes -= stepSize;
			//}

			//return results.toByteArray();

			return new byte[3];
		}

		protected abstract int GetIterationStartOffset ();
	}
}

