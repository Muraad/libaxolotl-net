using System;

namespace Axolotl
{
	public interface IDecryprionCallback
	{
		void HandlePlaintext (byte[] plaintext);
	}
}

