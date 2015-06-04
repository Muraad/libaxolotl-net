using System;

namespace Axolotl.Ratchet
{
	public class MessageKeys
	{
		// TODO: check if right

		public UInt32 Counter { get; set; }

		public byte[] CipherKey { get; set; }

		public byte[] MacKey { get; set; }

		public byte[] Iv { get; set; }

		public MessageKeys(byte[] cipherKey, byte[] macKey, byte[] iv, UInt32 counter)
		{
			CipherKey = cipherKey;
			MacKey = macKey;
			Iv = iv;
			Counter = (UInt32)counter;
		}
	}
}

