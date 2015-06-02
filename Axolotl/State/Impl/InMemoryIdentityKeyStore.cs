using System;
using System.Collections.Generic;

namespace Axolotl.State
{
	public class InMemoryIdentityKeyStore : IIdentityKeyStore
	{
		private Dictionary<string, IdentityKey> _trustedKeys;

		public IdentityKeyPair IdentityKeyPair { get; private set; }
		public int LocalRegistrationId { get; private set; }

		public InMemoryIdentityKeyStore (IdentityKeyPair identityKeyPair, int localRegistrationId)
		{
			_trustedKeys = new Dictionary<string, IdentityKey> ();
			IdentityKeyPair = identityKeyPair;
			LocalRegistrationId = localRegistrationId;
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
	}
}

