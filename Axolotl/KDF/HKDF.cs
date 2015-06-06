using System;
using System.Security.Cryptography;
using System.IO;

namespace Axolotl.KDF
{
	// Full Complete
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
				using(var hmac = new HMACSHA256(salt)){
					//TODO Check!
					var result = hmac.ComputeHash(inputKeyMaterial);
					//var result = hmac.TransformFinalBlock(inputKeyMaterial, 0, inputKeyMaterial.Length);
					return result;
				}
			}
			catch (Exception e) {
				throw new InvalidOperationException ("Assertion Error", e);
			}
		}

		private byte[] Expand (byte[] prk, byte[] info, int outputSize)
		{
			Console.WriteLine ("Output size is: {0}", outputSize);
			try {
				int                   iterations     = (int) Math.Ceiling((double) outputSize / (double) HASH_OUTPUT_SIZE);
				byte[]                mixin          = new byte[0];
				int                   remainingBytes = outputSize;

				Console.WriteLine ("Iterations: {0}", iterations);

				byte[] results;
				using(var stream = new MemoryStream())
				{
					for (var i= GetIterationStartOffset(); i < iterations + GetIterationStartOffset(); i++)
					{
						using(var hmac = new HMACSHA256(prk))
						{
							Console.WriteLine ("Remaining bytes: {0}", remainingBytes);

							hmac.TransformBlock(mixin, 0, mixin.Length, mixin, 0);

							if(info != null) {
								hmac.TransformBlock(info, 0, info.Length, info, 0);
							}
							//hmac.TransformFinalBlock(new byte[] { (byte)i }, 0, 1);

							var stepResult = hmac.ComputeHash(new byte[] { (byte)i });
							int    stepSize   = Math.Min(remainingBytes, stepResult.Length);

							stream.Write(stepResult, 0, stepSize);

							mixin          = stepResult;
							remainingBytes -= stepSize;
						}
					}
					results = stream.ToArray();
					Console.WriteLine ("Stream length: {0}", stream.Length);
				}
				return results;
			} catch (Exception e) {
				throw new InvalidOperationException("Assertion error", e);
			}
		}

		protected abstract int GetIterationStartOffset ();
	}
}

