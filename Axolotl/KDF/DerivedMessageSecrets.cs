using System;
using Axolotl.Util;
using System.Security.Cryptography;

namespace Axolotl.KDF
{
	public class DerivedMessageSecrets
	{
		public  static int SIZE              = 80;
		private static int CIPHER_KEY_LENGTH = 32;
		private static int MAC_KEY_LENGTH    = 32;
		private static int IV_LENGTH         = 16;

		public byte[] CipherKey { get; private set; }
		public byte[] MacKey { get; private set; }
		public byte[] Iv { get; private set; }

		public DerivedMessageSecrets (byte[] okm)
		{
			try {
				byte[][] keys = ByteUtil.Split(okm, CIPHER_KEY_LENGTH, MAC_KEY_LENGTH, IV_LENGTH);
				//cipherKey = 
				//macKey = new HMACSHA256(keys[1]).Key;
				// TODO
			}
			catch {
			}
		}
	}
}

