using System;
using System.Collections.Generic;

namespace Axolotl.State
{
	public class InMemoryPreKeyStore : IPreKeyStore
	{
		private Dictionary<UInt32, byte[]> _store;

		public InMemoryPreKeyStore ()
		{
			_store = new Dictionary<uint, byte[]> ();
		}

		public PreKeyRecord LoadPreKey(uint preKeyId) 
		{
			if (!_store.ContainsKey (preKeyId)) {
				throw new KeyNotFoundException ("No such preKeyRecord");
			}

			return new PreKeyRecord (_store [preKeyId]);
			//TODO: add check
		}

		public void StorePreKey(uint preKeyId, PreKeyRecord record) 
		{
			_store.Add (preKeyId, record.Serialize());
		}

		public bool ContainsPreKey(uint preKeyId)
		{
			return _store.ContainsKey (preKeyId);
		}

		public void RemovePreKey(uint preKeyId)
		{
			_store.Remove (preKeyId);
		}
	}
}

