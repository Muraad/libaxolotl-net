using System;
using System.Collections.Generic;

namespace Axolotl.State
{
	public class InMemorySignedPreKeyStore : ISignedPreKeyStore
	{
		private Dictionary<UInt32, byte[]> _store;

		public InMemorySignedPreKeyStore ()
		{
			_store = new Dictionary<UInt32, byte[]> ();
		}

		public SignedPreKeyRecord LoadSignedPreKey(UInt32 signedPreKeyId) 
		{
			// TODO: exceptions check
			if (!_store.ContainsKey(signedPreKeyId)) {
				throw new KeyNotFoundException("No such signedprekeyrecord! " + signedPreKeyId);
			}
			return new SignedPreKeyRecord(_store[signedPreKeyId]);
		}

		public List<SignedPreKeyRecord> LoadSignedPreKeys() 
		{
			//TODO: check
			var results = new List<SignedPreKeyRecord>();

			foreach(var serialized in _store.Values) {
				results.Add(new SignedPreKeyRecord(serialized));
			}

			return results;
		}

		public void StoreSignedPreKey(UInt32 signedPreKeyId, SignedPreKeyRecord record) {
			_store.Add(signedPreKeyId, record.Serialize());
		}

		public bool ContainsSignedPreKey(UInt32 signedPreKeyId) {
			return _store.ContainsKey(signedPreKeyId);
		}

		public void RemoveSignedPreKey(UInt32 signedPreKeyId) {
			_store.Remove(signedPreKeyId);
		}
	}
}

