using NUnit.Framework;
using System;
using Axolotl.Groups.State;
using System.Collections.Generic;

namespace Axolotl.Groups
{
	public class InMemorySenderKeyStore : ISenderKeyStore 
	{
		Dictionary<SenderKeyName, SenderKeyRecord> store;

		public InMemorySenderKeyStore ()
		{
			store = new Dictionary<SenderKeyName, SenderKeyRecord>();
		}

		public void StoreSenderKey(SenderKeyName senderKeyName, SenderKeyRecord record) {
			if (store.ContainsKey (senderKeyName)) {
				store [senderKeyName] = record;
			} else {
				store.Add (senderKeyName, record);
			}
		}

		public SenderKeyRecord LoadSenderKey(SenderKeyName senderKeyName) {
			try {
				if (store.ContainsKey(senderKeyName)) {
					var record = store[senderKeyName];
					return new SenderKeyRecord(record.Serialize());
				} else {
					return new SenderKeyRecord();
				}
			} catch (Exception e) {
				throw new InvalidOperationException("Assertion error", e);
			}
		}
	}
}

