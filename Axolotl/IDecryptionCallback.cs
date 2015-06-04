using System;

namespace Axolotl
{
	public interface IDecryptionCallback
	{
		void HandlePlaintext(byte[] plaintext);
	}
}

