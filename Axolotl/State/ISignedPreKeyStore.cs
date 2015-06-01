using System;
using System.Collections.Generic;

namespace Axolotl.State
{
	public interface ISignedPreKeyStore
	{
		SignedPreKeyRecord LoadSignedPreKey (int signedPreKeyId);

		List<SignedPreKeyRecord> LoadSignedPreKeys ();

		void StoreSignedPreKey (int signedPreKeyId, SignedPreKeyRecord record);

		bool ContainsSignedPreKey (int signedPreKeyId);

		void RemoveSignedPreKey (int signedPreKeyId);
	}
}

