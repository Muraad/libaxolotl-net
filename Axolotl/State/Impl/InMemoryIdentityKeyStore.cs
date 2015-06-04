using System;
using System.Collections.Generic;

namespace Axolotl.State
{
	public class InMemoryIdentityKeyStore : IIdentityKeyStore
	{
		private Dictionary<string, IdentityKey> _trustedKeys;

		private IdentityKeyPair _identityKeyPair;

		private UInt32 _localRegistrationId;

		public InMemoryIdentityKeyStore (IdentityKeyPair identityKeyPair, UInt32 localRegistrationId)
		{
			_trustedKeys = new Dictionary<string, IdentityKey> ();
			_identityKeyPair = identityKeyPair;
			_localRegistrationId = localRegistrationId;
		}

		public void SaveIdentity(string name, IdentityKey identityKey) 
		{
			_trustedKeys.Add(name, identityKey);
		}

		public bool IsTrustedIdentity(string name, IdentityKey identityKey) 
		{
			IdentityKey trusted = _trustedKeys[name];
			return (trusted == null || trusted.Equals(identityKey));
		}

		public IdentityKeyPair GetIdentityKeyPair()
		{
			return _identityKeyPair;
		}

		public UInt32 GetLocalRegistrationId()
		{
			return _localRegistrationId;
		}
	}
}

