using System;
using System.Text;
using Axolotl.KDF;
using Axolotl.Util;

namespace Axolotl.Groups.Ratchet
{
	public class SenderMessageKey
	{
		public UInt32    Iteration;
		public byte[] Iv;
		public byte[] CipherKey;
		public byte[] Seed;

		public SenderMessageKey (UInt32 iteration, byte[] seed)
		{
			byte[] derivative = new HKDFv3 ().DeriveSecrets (seed, Encoding.UTF8.GetBytes ("WhisperGroup"), 48);
			byte[][] parts = ByteUtil.Split (derivative, 16, 32);

			Iteration = iteration;
			Seed = seed;
			Iv = parts [0];
			CipherKey = parts [1];
		}
	}
}

