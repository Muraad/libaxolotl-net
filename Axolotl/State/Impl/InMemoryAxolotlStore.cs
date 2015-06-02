using System;
using System.Collections.Generic;

namespace Axolotl.State
{
	public class InMemoryAxolotlStore : IAxolotlStore
	{
		private InMemoryPreKeyStore       _preKeyStore;
		private InMemorySessionStore      _sessionStore;
		private InMemorySignedPreKeyStore _signedPreKeyStore;

		private InMemoryIdentityKeyStore  _identityKeyStore;

		public InMemoryAxolotlStore(IdentityKeyPair identityKeyPair, int registrationId) 
		{
			_identityKeyStore = new InMemoryIdentityKeyStore(identityKeyPair, registrationId);
			_preKeyStore = new InMemoryPreKeyStore ();
			_sessionStore = new InMemorySessionStore ();
			_signedPreKeyStore = new InMemorySignedPreKeyStore ();
		}

		public IdentityKeyPair IdentityKeyPair {
			get { return _identityKeyStore.IdentityKeyPair; }
		}

		public int LocalRegistrationId {
			get { return _identityKeyStore.LocalRegistrationId; }
		}

		public void SaveIdentity(string name, IdentityKey identityKey) 
		{
			_identityKeyStore.SaveIdentity(name, identityKey);
		}

		public bool IsTrustedIdentity(string name, IdentityKey identityKey) 
		{
			return _identityKeyStore.IsTrustedIdentity(name, identityKey);
		}

		public PreKeyRecord LoadPreKey(uint preKeyId)
		{
			return _preKeyStore.LoadPreKey(preKeyId);
		}

		public void StorePreKey(uint preKeyId, PreKeyRecord record) 
		{
			_preKeyStore.StorePreKey(preKeyId, record);
		}

		public bool ContainsPreKey(uint preKeyId) {
			return _preKeyStore.ContainsPreKey(preKeyId);
		}

		public void RemovePreKey(uint preKeyId) {
			_preKeyStore.RemovePreKey(preKeyId);
		}

		public SessionRecord LoadSession(AxolotlAddress address) {
			return _sessionStore.LoadSession(address);
		}

		public List<int> GetSubDeviceSessions(string name) {
			return _sessionStore.GetSubDeviceSessions(name);
		}

		public void StoreSession(AxolotlAddress address, SessionRecord record) {
			_sessionStore.StoreSession(address, record);
		}

		public bool ContainsSession(AxolotlAddress address) {
			return _sessionStore.ContainsSession(address);
		}

		public void DeleteSession(AxolotlAddress address) {
			_sessionStore.DeleteSession(address);
		}

		public void DeleteAllSessions(string name) {
			_sessionStore.DeleteAllSessions(name);
		}

		public SignedPreKeyRecord LoadSignedPreKey(int signedPreKeyId)
		{
			return _signedPreKeyStore.LoadSignedPreKey(signedPreKeyId);
		}

		public List<SignedPreKeyRecord> LoadSignedPreKeys() {
			return _signedPreKeyStore.LoadSignedPreKeys();
		}

		public void StoreSignedPreKey(int signedPreKeyId, SignedPreKeyRecord record) {
			_signedPreKeyStore.StoreSignedPreKey(signedPreKeyId, record);
		}

		public bool ContainsSignedPreKey(int signedPreKeyId) {
			return _signedPreKeyStore.ContainsSignedPreKey(signedPreKeyId);
		}

		public void RemoveSignedPreKey(int signedPreKeyId) {
			_signedPreKeyStore.RemoveSignedPreKey(signedPreKeyId);
		}
	}
}

