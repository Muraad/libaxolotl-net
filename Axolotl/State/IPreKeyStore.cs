using System;

namespace Axolotl.State
{
	public interface IPreKeyStore
	{
		PreKeyRecord LoadPreKey (int preKeyId);
		void StorePreKey(int preKeyId, PreKeyRecord record);
		bool ContainsPreKey(int preKeyId, PreKeyRecord record);
		void RemovePreKey (int preKeyId);
	}
}

