using System;

namespace Axolotl
{
	public interface IDecryprionCallback
	{
		void HandlePlainText(byte[] plaintext);
	}
}

