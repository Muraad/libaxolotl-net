using System;

namespace Axolotl.State
{
	public interface IPreKeyStore
	{
		PreKeyRecord LoadPreKey (uint preKeyId);
		void StorePreKey(uint preKeyId, PreKeyRecord record);
		bool ContainsPreKey(uint preKeyId);
		void RemovePreKey (uint preKeyId);
	}
}

