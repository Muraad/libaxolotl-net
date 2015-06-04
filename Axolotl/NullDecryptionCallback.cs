using System;

namespace Axolotl
{
	public class NullDecryptionCallback : IDecryptionCallback
	{
		public void HandlePlaintext(byte[] plaintext)
		{
		}
	}
}

