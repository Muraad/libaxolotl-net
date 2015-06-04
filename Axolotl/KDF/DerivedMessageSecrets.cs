using System;
using Axolotl.Util;
using System.Security.Cryptography;

namespace Axolotl.KDF
{
	public class DerivedMessageSecrets
	{
		public  const int SIZE              = 80;
		private const int CIPHER_KEY_LENGTH = 32;
		private const int MAC_KEY_LENGTH    = 32;
		private const int IV_LENGTH         = 16;

		public byte[] CipherKey { get; private set; }
		public byte[] MacKey { get; private set; }
		public byte[] Iv { get; private set; }

		public DerivedMessageSecrets (byte[] okm)
		{
			try {
				//byte[][] keys = ByteUtil.Split(okm, CIPHER_KEY_LENGTH, MAC_KEY_LENGTH, IV_LENGTH);
				//cipherKey = 
				//macKey = new HMACSHA256(keys[1]).Key;
				// TODO
			}
			catch {
			}
		}
	}
}

