using System;
using System.Collections.Generic;

namespace Axolotl.State
{
	public interface ISignedPreKeyStore
	{
		// Complete
		SignedPreKeyRecord LoadSignedPreKey (UInt32 signedPreKeyId);

		List<SignedPreKeyRecord> LoadSignedPreKeys ();

		void StoreSignedPreKey (UInt32 signedPreKeyId, SignedPreKeyRecord record);

		bool ContainsSignedPreKey (UInt32 signedPreKeyId);

		void RemoveSignedPreKey (UInt32 signedPreKeyId);
	}
}

